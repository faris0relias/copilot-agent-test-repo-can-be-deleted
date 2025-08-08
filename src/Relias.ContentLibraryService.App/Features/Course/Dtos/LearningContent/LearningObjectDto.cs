using Relias.ContentLibraryService.Domain.Course.LearningContent;
using System.Text.Json.Serialization;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(LessonDto), "lesson")]
[Newtonsoft.Json.JsonConverter(typeof(LearningObjectDtoNewtonsoftConverter))] // Newtonsoft.Json attribute for JsonPatch
public class LearningObjectDto
{
    public LearningObjectType LearningObjectType { get; set; }
    public Guid LearningObjectId { get; set; }
}
