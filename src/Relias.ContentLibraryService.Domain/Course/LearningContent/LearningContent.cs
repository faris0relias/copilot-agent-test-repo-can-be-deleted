namespace Relias.ContentLibraryService.Domain.Course.LearningContent;

public class LearningContent
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public List<LearningContentSection> Sections { get; set; } = [];
}