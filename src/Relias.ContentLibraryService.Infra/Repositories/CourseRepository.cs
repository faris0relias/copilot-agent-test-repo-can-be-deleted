using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Domain.Content;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Infra.Persistence;

namespace Relias.ContentLibraryService.Infra.Repositories;

public class CourseRepository(ILogger<CourseRepository> logger, ApplicationDbContext dbContext) : ICourseRepository
{
    public async Task<IEnumerable<Course>> GetCoursesAsync(int organizationId, CancellationToken cancellationToken)
    {
        return await dbContext.Courses
                .Where(c => c.OrganizationId == organizationId)
                .ToListAsync(cancellationToken);
    }

    public async Task<Course?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await dbContext.Courses
            .FirstOrDefaultAsync(c => c.CourseId == courseId, cancellationToken);

        if (course?.IsRelias == true)
        {
            await dbContext.Entry(course)
                .Reference(c => c.ReliasCourseProperties)
                .Query()
                .Include(rp => rp.CommercialProductDisclaimer)
                .Include(rp => rp.CompletionRequirement)
                .Include(rp => rp.ContentDisclaimer)
                .Include(rp => rp.CulturalAwarenessStatement)
                .Include(rp => rp.RequestForAccommodations)
                .Include(rp => rp.LearningObjectives)
                .Include(rp => rp.Contributors).ThenInclude(cc => cc.Contributor)
                .Include(rp => rp.CareSettings).ThenInclude(cs => cs.CareSetting)
                .Include(rp => rp.TargetAudiences).ThenInclude(ta => ta.TargetAudience)
                .Include(rp => rp.TrainingTopics).ThenInclude(tt => tt.TrainingTopic)
                .Include(rp => rp.Disclosures).ThenInclude(dc => dc.Disclosure)
                .LoadAsync(cancellationToken);
        }

        return course;
    }

    public async Task<Course?> GetCourseByCourseIdAsync(Guid courseId, int organizationId, CancellationToken cancellationToken)
    {
        return await dbContext.Courses
            .Where(c => c.CourseId == courseId && c.OrganizationId == organizationId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetCoursesByContentIdsAsync(IEnumerable<Guid> contentIds, CancellationToken cancellationToken)
    {
        return await dbContext.Courses
            .Where(c => c.ContentId.HasValue && contentIds.ToList().Contains(c.ContentId.Value))
            .ToListAsync(cancellationToken);
    }

    public async Task<Course?> FindByContentCodeAsync(string contentCode, Guid excludeCourseId, CancellationToken cancellationToken)
    {
        return await dbContext.Courses
            .FirstOrDefaultAsync(c => c.ContentCode == contentCode && c.CourseId != excludeCourseId, cancellationToken);
    }

    public async Task<Content?> GetContentByCourseAsync(Guid contentId, CancellationToken cancellationToken)
    {
        return await dbContext.Content
            .FirstOrDefaultAsync(c => c.ContentId == contentId, cancellationToken);
    }

    public async Task<Course> CreateCourseAsync(CreateCourseDto newCourse, CancellationToken cancellationToken)
    {
        var courseId = Guid.NewGuid();

        var isContentCodeDuplicate = await dbContext.Courses.AnyAsync(c => c.ContentCode == newCourse.ContentCode, cancellationToken);

        if (isContentCodeDuplicate)
        {
            logger.LogDebug("Unable to create new course with CourseId {CourseId}, content code already exists.", courseId);
            throw new BadRequestException(newCourse.ContentCode, "Value", "The content code is already in use.");
        }

        try
        {
            await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var courseContentType = await dbContext.ContentType.FirstOrDefaultAsync(ct => ct.ContentTypeId == 2, cancellationToken);

            if (courseContentType == null)
            {
                logger.LogDebug("Unable to create new course with CourseId {CourseId}, content type 'Course' not found.", courseId);
                throw new NotFoundException("ContentType Course not found.");
            }

            Content content = new()
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId
            };

            await dbContext.AddAsync(content, cancellationToken);

            Course course = new()
            {
                CourseId = courseId,
                ContentId = content.ContentId,
                Title = newCourse.CourseName,
                OrganizationId = newCourse.OrganizationId,
                ContentCode = newCourse.ContentCode.ToUpper(),
                Created = DateTime.Now,
                StatusId = 1
            };

            await dbContext.Courses.AddAsync(course, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await dbContext.Database.CommitTransactionAsync(cancellationToken);

            return course;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong trying to create Course with Course ID {CourseId}.", courseId);

            await dbContext.Database.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(Course updateCourse, CancellationToken cancellationToken)
    {
        dbContext.Courses.Update(updateCourse);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task ArchiveAsync(Course course, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("ArchiveCourse is not implemented yet.");
    }

    public async Task DeleteAsync(Course course, Content? content, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

            dbContext.Courses.Remove(course);

            if (content is not null)
            {
                dbContext.Content.Remove(content);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await dbContext.Database.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete course or content. Rolling back.");
            await dbContext.Database.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task TriggerCourseContentUpdateAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await GetByIdAsync(courseId, cancellationToken) ?? throw new NotFoundException($"Failed to update course metadata: Course with ID {courseId} not found.");
        await UpdateAsync(course, cancellationToken);
    }
}
