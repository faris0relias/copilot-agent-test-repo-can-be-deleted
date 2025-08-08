using AutoMapper;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.Common.Resilience;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Common.Exceptions;
using FluentValidation.Results;
using Relias.ContentLibraryService.Common.Enums;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Common.Interfaces;

namespace Relias.ContentLibraryService.App.Services;

public class CourseService(
    ICourseRepository repository,
    ILearningContentService learningContentService,
    IFinalExamService finalExamService,
    IMapper mapper,
    ILogger<CourseService> logger,
    IResilienceExecutor resilienceExecutor,
    IScheduledCourseRepository scheduledCourseRepository,
    IUnitOfWork unitOfWork
) : ICourseService
{
    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto newCourse, CancellationToken cancellationToken)
    {
        var course = await repository.CreateCourseAsync(newCourse, cancellationToken);

        try
        {
            await resilienceExecutor.ExecuteAsync(
                ct => learningContentService.InitializeByCourseIdAsync(course.CourseId, ct),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "All retries failed. LearningContent initialization will be attempted later for CourseId {CourseId}.",
                course.CourseId);
        }

        return mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await repository.GetByIdAsync(courseId, cancellationToken);

        return mapper.Map<CourseDto>(course);
    }

    public async Task<IEnumerable<CourseDto>?> GetCoursesAsync(int orgId, CancellationToken cancellationToken)
    {
        var courses = await repository.GetCoursesAsync(orgId, cancellationToken);

        return mapper.Map<IEnumerable<CourseDto>>(courses);

    }

    public async Task<CourseDto> UpdateAsync(Guid courseId, UpdateCourseDto updatedCourse, CancellationToken cancellationToken)
    {
        var existingCourse = await repository.GetByIdAsync(courseId, cancellationToken);
        if (existingCourse is null)
        {
            logger.LogDebug("Unable to locate a course with ID {CourseId}", courseId);
            throw new NotFoundException($"Course with ID {courseId} not found.");
        }

        var duplicate = await repository.FindByContentCodeAsync(updatedCourse.ContentCode, existingCourse.CourseId, cancellationToken);
        if (duplicate != null)
        {
            logger.LogDebug("Unable to update course with ID {CourseId}, content code already exists.", courseId);
            throw new BadRequestException(updatedCourse.ContentCode, "Value", "The content code is already in use.");
        }

        try
        {
            existingCourse.Title = updatedCourse.Title;
            existingCourse.ContentId = updatedCourse.ContentId;
            existingCourse.ContentCode = updatedCourse.ContentCode;
            existingCourse.BriefDescription = updatedCourse.BriefDescription;
            existingCourse.Description = updatedCourse.Description;
            existingCourse.OrganizationId = updatedCourse.OrganizationId;
            existingCourse.LanguageIds = updatedCourse.LanguageIds;

            await repository.UpdateAsync(existingCourse, cancellationToken);

            return mapper.Map<CourseDto>(existingCourse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong trying to update Course with Course ID {courseId}. Message: {message}.", courseId, ex.Message);
            throw;
        }
    }

    public async Task DeleteAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await repository.GetByIdAsync(courseId, cancellationToken);
        if (course is null)
        {
            logger.LogDebug("Course not found with ID {CourseId}", courseId);
            throw new NotFoundException("Course not found.");
        }

        if (course.StatusId != 1)
        {
            var failures = new List<ValidationFailure>
            {
                new ()
                {
                    AttemptedValue = course.StatusId,
                    ErrorMessage = "Only courses in draft status can be deleted.",
                    PropertyName = nameof(course.StatusId)
                }
            };

            logger.LogDebug("Cannot delete Course {CourseId} because it's not in Draft status", courseId);
            throw new ValidationException(failures);
        }

        Domain.Content.Content? content = null;
        if (course.ContentId.HasValue)
        {
            content = await repository.GetContentByCourseAsync(course.ContentId.Value, cancellationToken);
            if (content is null)
            {
                logger.LogDebug("Content not found for Course {CourseId} with ContentId {ContentId}", courseId, course.ContentId);
            }
        }

        await repository.DeleteAsync(course, content, cancellationToken);

        try
        {
            await resilienceExecutor.ExecuteAsync(
                ct => learningContentService.DeleteByCourseIdAsync(course.CourseId, ct),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to delete Learning Content for CourseId {CourseId}.",
                course.CourseId);
        }

        try
        {
            await resilienceExecutor.ExecuteAsync(
                ct => finalExamService.DeleteFinalExamAsync(course.CourseId, ct),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to delete Final Exam for CourseId {CourseId}.",
                course.CourseId);
        }
    }

    public async Task PublishAsync(Guid courseId, PublishCourseDto dto, CancellationToken cancellationToken)
    {
        var newStatus = (StatusIdEnums)dto.StatusId;

        var course = await ValidateCourseForPublishingAsync(courseId, newStatus, cancellationToken);

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await scheduledCourseRepository.DeleteByCourseIdAndStatusIdAsync(courseId, (int)StatusIdEnums.ScheduledForPublish, cancellationToken);

            if (newStatus == StatusIdEnums.ScheduledForPublish)
            {
                await ScheduleCourseAsync(courseId, dto, cancellationToken);
            }

            course.StatusId = (byte)newStatus;

            if (newStatus == StatusIdEnums.Published)
            {
                course.PublishDate = DateTime.UtcNow;
                course.PublishBy = dto.PublishBy;
            }
            
            await repository.UpdateAsync(course, cancellationToken);

        }, cancellationToken);

        if (newStatus == StatusIdEnums.Published)
        {
            // TODO: Send CoursePublished event
        }
    }

    private async Task<Course> ValidateCourseForPublishingAsync(Guid courseId, StatusIdEnums newStatus, CancellationToken cancellationToken)
    {
        var course = await repository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException($"Course with ID {courseId} not found.");

        if (newStatus == StatusIdEnums.Published && course.StatusId == (int)StatusIdEnums.Published)
        {
            throw new InvalidOperationException($"Course with CourseId {courseId} is already published.");
        }

        return course;
    }

    private async Task ScheduleCourseAsync(Guid courseId, PublishCourseDto dto, CancellationToken cancellationToken)
    {
        var scheduledCourse = new ScheduledCourse
        {
            CourseId = courseId,
            StatusId = dto.StatusId,
            ScheduledDate = dto.PublishDate ?? throw new InvalidOperationException("PublishDate must be provided when scheduling."),
            ScheduledBy = dto.PublishBy
        };

        await scheduledCourseRepository.CreateAsync(scheduledCourse, cancellationToken);
    }

    public Task ArchiveAsync(Guid courseId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
