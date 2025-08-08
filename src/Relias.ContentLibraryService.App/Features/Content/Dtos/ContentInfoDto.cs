namespace Relias.ContentLibraryService.App.Features.Content.Dtos;

public class ContentInfoDto
{
    public Guid ContentId { get; set; }
    public int ContentTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? TimeToCompleteInMinutes { get; set; }
}
