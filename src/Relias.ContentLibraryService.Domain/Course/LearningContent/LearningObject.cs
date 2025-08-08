using System.Text.Json.Serialization;

namespace Relias.ContentLibraryService.Domain.Course.LearningContent;

public class LearningObject
{
    public LearningObjectType LearningObjectType { get; set; }
    public Guid LearningObjectId { get; set; }
}
