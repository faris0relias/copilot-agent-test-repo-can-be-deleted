namespace Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;

public class LearningContentDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public List<LearningContentSectionDto> Sections { get; set; } = [];
}
