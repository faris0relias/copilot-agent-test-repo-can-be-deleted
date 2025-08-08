namespace Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;

public class LearningContentSectionDto
{
    public Guid SectionId { get; set; }
    public required LocalizedStringDto Name { get; set; }
    public List<LearningObjectDto> LearningObjects { get; set; } = [];
}
