namespace Relias.ContentLibraryService.App.Features.Course.Dtos;

public class PublishCourseDto
{
    public int StatusId { get; init; }
    public DateTime? PublishDate { get; set; }
    public string? PublishBy { get; set; }
}
