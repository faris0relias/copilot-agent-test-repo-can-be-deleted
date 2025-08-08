using Relias.ContentLibraryService.Domain.Course;

namespace Relias.ContentLibraryService.App.Interfaces;

public interface IScheduledCourseRepository
{
    Task CreateAsync(ScheduledCourse scheduledCourse, CancellationToken cancellationToken);
    Task DeleteByCourseIdAndStatusIdAsync(Guid courseId, int statusId, CancellationToken cancellationToken);
}
