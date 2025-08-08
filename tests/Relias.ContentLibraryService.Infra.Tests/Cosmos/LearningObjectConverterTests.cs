using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Relias.ContentLibraryService.Infra.Cosmos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Relias.ContentLibraryService.Infra.Tests.Cosmos;

public class LearningObjectConverterTests
{
    private readonly JsonSerializerOptions _options;

    public LearningObjectConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        _options.Converters.Add(new LearningObjectConverter());
    }

    [Fact]
    public void Deserialize_WhenMissingDiscriminator_ThrowsJsonException()
    {
        var json = """{ "learningObjectId": "a3f8dead-1096-44df-9fd3-dce14726b8f2" }""";

        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<LearningObject>(json, _options));

        Assert.Contains("learningObjectType", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Deserialize_WhenUnsupportedLearningObjectType_ThrowsJsonException()
    {
        var json = """{ "learningObjectType": 999, "learningObjectId": "a3f8dead-1096-44df-9fd3-dce14726b8f2" }""";

        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<LearningObject>(json, _options));

        Assert.Contains("Unsupported LearningObjectType", exception.Message);
    }

    [Fact]
    public void Deserialize_WhenValidLessonJson_ReturnsLesson()
    {
        var json = """
        {
            "learningObjectType": 0,
            "learningObjectId": "a3f8dead-1096-44df-9fd3-dce14726b8f2",
            "lessonType": "video",
            "name": { "en": "Lesson 1" },
            "durationMinutes": 10,
            "requiredForCompletion": true,
            "requiresAudio": false,
            "requiresVideo": true
        }
        """;

        var result = JsonSerializer.Deserialize<LearningObject>(json, _options);

        Assert.NotNull(result);
        Assert.IsType<Lesson>(result);

        var lesson = (Lesson)result!;
        Assert.Equal(Guid.Parse("a3f8dead-1096-44df-9fd3-dce14726b8f2"), lesson.LearningObjectId);
        Assert.Equal("video", lesson.LessonType);
        Assert.Equal("Lesson 1", lesson.Name.En);
    }

    [Fact]
    public void Serialize_WhenLessonProvided_ReturnsValidJson()
    {
        var lesson = new Lesson
        {
            LearningObjectId = Guid.NewGuid(),
            LearningObjectType = LearningObjectType.Lesson,
            LessonType = "video",
            Name = new LocalizedString { En = "Intro Lesson" },
            DurationMinutes = 15,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = true
        };

        var json = JsonSerializer.Serialize<LearningObject>(lesson, _options);

        Assert.Contains("\"learningObjectType\":0", json);
        Assert.Contains("\"lessonType\":\"video\"", json);
        Assert.Contains("\"name\"", json);
        Assert.Contains("\"en\":\"Intro Lesson\"", json);
    }

    [Fact]
    public void Serialize_WhenUnsupportedTypeProvided_ThrowsJsonException()
    {
        var unsupportedObject = new MockLearningObject
        {
            LearningObjectId = Guid.NewGuid(),
            LearningObjectType = (LearningObjectType)99
        };

        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Serialize<LearningObject>(unsupportedObject, _options));

        Assert.Contains("Unsupported LearningObject type", exception.Message);
    }

    private class MockLearningObject : LearningObject { }
}