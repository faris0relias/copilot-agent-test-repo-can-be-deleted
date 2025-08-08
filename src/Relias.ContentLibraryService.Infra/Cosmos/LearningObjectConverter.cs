using Relias.ContentLibraryService.Domain.Course.LearningContent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Relias.ContentLibraryService.Infra.Cosmos;

public class LearningObjectConverter : JsonConverter<LearningObject>
{
    public override LearningObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        if (!root.TryGetProperty("learningObjectType", out var typeProp))
            throw new JsonException("Missing learningObjectType discriminator");

        var typeValue = typeProp.GetInt32();
        var learningObjectType = (LearningObjectType)typeValue;

        return learningObjectType switch
        {
            LearningObjectType.Lesson => JsonSerializer.Deserialize<Lesson>(root.GetRawText(), options),
            _ => throw new JsonException($"Unsupported LearningObjectType: {learningObjectType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, LearningObject value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case Lesson lesson:
                JsonSerializer.Serialize(writer, lesson, options);
                break;
            default:
                throw new JsonException($"Unsupported LearningObject type: {value.GetType().Name}");
        }
    }
}