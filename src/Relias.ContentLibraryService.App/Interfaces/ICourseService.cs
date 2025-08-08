using Relias.ContentLibraryService.App.Features.Course.Dtos;

namespace Relias.ContentLibraryService.App.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>?> GetCoursesAsync(int orgId, CancellationToken cancellationToken);
    Task<CourseDto?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken);
    Task<CourseDto> CreateCourseAsync(CreateCourseDto newCourse, CancellationToken cancellationToken);
    Task<CourseDto> UpdateAsync(Guid courseId, UpdateCourseDto updatedCourse, CancellationToken cancellationToken);
    Task ArchiveAsync(Guid courseId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid courseId, CancellationToken cancellationToken);
    Task PublishAsync(Guid courseId, PublishCourseDto publishCourse, CancellationToken cancellationToken);
}
