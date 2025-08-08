namespace Relias.ContentLibraryService.App.Features.Course.Dtos;

public class UpdateCourseDto
{
    public Guid? ContentId { get; init; }
    public int OrganizationId { get; init; }
    public required string Title { get; init; }
    public required string ContentCode { get; init; }   
    public string? Description { get; init; } = string.Empty;
    public string? BriefDescription { get; init; } = string.Empty;
    public List<Guid>? LanguageIds { get; init; }
}
