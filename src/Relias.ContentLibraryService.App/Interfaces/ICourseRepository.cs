using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.Domain.Course;

namespace Relias.ContentLibraryService.App.Interfaces;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetCoursesAsync(int organizationId, CancellationToken cancellationToken);
    Task<Course?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken);
    Task<Course?> FindByContentCodeAsync(string contentCode, Guid excludeCourseId, CancellationToken cancellationToken);
    Task<Domain.Content.Content?> GetContentByCourseAsync(Guid contentId, CancellationToken cancellationToken);
    Task<Course?> GetCourseByCourseIdAsync(Guid courseId, int organizationId, CancellationToken cancellationToken);
    Task<IEnumerable<Course>> GetCoursesByContentIdsAsync(IEnumerable<Guid> contentIds, CancellationToken cancellationToken);
    Task<Course> CreateCourseAsync(CreateCourseDto newCourse, CancellationToken cancellationToken);
    Task UpdateAsync(Course updateCourse, CancellationToken cancellationToken);
    Task ArchiveAsync(Course course, CancellationToken cancellationToken);
    Task DeleteAsync(Course course, Domain.Content.Content? content, CancellationToken cancellationToken);
    Task TriggerCourseContentUpdateAsync(Guid courseId, CancellationToken cancellationToken);
}