namespace Relias.ContentLibraryService.App.Features.Course.Dtos;

public class CreateCourseDto
{
    public int OrganizationId { get; init; }
    public required string CourseName { get; init; }
    public required string ContentCode { get; set; }
}
