using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;

/// <summary>
/// Custom Newtonsoft.Json converter for polymorphic deserialization of LearningObjectDto.
/// This converter uses the 'learningObjectType' property to determine the concrete type.
/// It is primarily used by JsonPatchDocument.ApplyTo.
/// </summary>
/// TODO: remove excludeFromCodeCoverage attribute when Lesson BDD integration tests are written - the ReadJson method is used when deserializing LearningObjects
[ExcludeFromCodeCoverage]
public class LearningObjectDtoNewtonsoftConverter : Newtonsoft.Json.JsonConverter<LearningObjectDto>
{
    private const string DiscriminatorPropertyKey = "learningObjectType";

    // We only need to customize ReadJson for JsonPatchDocument.ApplyTo.
    public override bool CanWrite => false;

    public override LearningObjectDto ReadJson(JsonReader reader, Type objectType, LearningObjectDto? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null!; // Or handle as per specific nullability requirements
        }

        JObject jo = JObject.Load(reader);
        JToken? discriminatorToken = jo[DiscriminatorPropertyKey];

        if (discriminatorToken == null || discriminatorToken.Type == JTokenType.Null)
        {
            throw new JsonSerializationException($"Property '{DiscriminatorPropertyKey}' not found or is null. Cannot deserialize polymorphic type {nameof(LearningObjectDto)} without this discriminator.");
        }

        LearningObjectType typeEnum;
        try
        {
            // Newtonsoft.Json deserializes enums from integers by default.
            // If the enum is sent as a string, ToObject<T> can also handle that if configured or if the string matches enum member names.
            typeEnum = discriminatorToken.ToObject<LearningObjectType>(serializer);
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException($"Error deserializing '{DiscriminatorPropertyKey}' for {nameof(LearningObjectDto)}. Value was: '{discriminatorToken}'. See inner exception for details.", ex);
        }

        LearningObjectDto targetDto;
        switch (typeEnum)
        {
            case LearningObjectType.Lesson:
                targetDto = new LessonDto
                {
                    LessonType = nameof(LearningObjectType.Lesson),
                    Name = new LocalizedStringDto()
                };
                break;
            // TODO: Add cases for other derived types of LearningObjectDto based on the LearningObjectType enum
            // Example:
            default:
                throw new JsonSerializationException($"Unsupported {nameof(LearningObjectType)} '{typeEnum}' encountered when trying to deserialize {nameof(LearningObjectDto)}.");
        }

        // Populate the properties onto the instantiated target DTO
        using (JsonReader jObjectReader = jo.CreateReader())
        {
            serializer.Populate(jObjectReader, targetDto);
        }
        return targetDto;
    }

    public override void WriteJson(JsonWriter writer, LearningObjectDto? value, JsonSerializer serializer)
    {
        // This method will not be called because CanWrite is false.
        // Newtonsoft.Json will use its default serialization for the actual type of 'value'.
        throw new NotImplementedException("This WriteJson method should not be called as CanWrite is false.");
    }
}