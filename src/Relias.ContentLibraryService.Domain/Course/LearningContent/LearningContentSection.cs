namespace Relias.ContentLibraryService.Domain.Course.LearningContent;

public class LearningContentSection
{
    public Guid SectionId { get; set; }
    public required LocalizedString Name { get; set; }
    public List<LearningObject> LearningObjects { get; set; } = [];
}
