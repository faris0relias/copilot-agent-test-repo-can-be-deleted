using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.Domain.Course;

namespace Relias.ContentLibraryService.Integration.Tests.Context;

public class CourseContext
{
    public Course? Course { get; set; }
    public List<Course>? Courses { get; set; }
    public CourseDto? CreatedCourseDto { get; set; }
}
