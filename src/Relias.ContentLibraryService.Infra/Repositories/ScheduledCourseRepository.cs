using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Infra.Persistence;

namespace Relias.ContentLibraryService.Infra.Repositories;

public class ScheduledCourseRepository(ApplicationDbContext dbContext) : IScheduledCourseRepository
{
    public async Task CreateAsync(ScheduledCourse scheduledCourse, CancellationToken cancellationToken)
    {
        await dbContext.ScheduledCourses.AddAsync(scheduledCourse, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByCourseIdAndStatusIdAsync(Guid courseId, int statusId, CancellationToken cancellationToken)
    {
        var existing = await dbContext.ScheduledCourses.FirstOrDefaultAsync(sc => sc.CourseId == courseId && sc.StatusId == statusId, cancellationToken);

        if (existing is not null)
        {
            dbContext.ScheduledCourses.Remove(existing);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
