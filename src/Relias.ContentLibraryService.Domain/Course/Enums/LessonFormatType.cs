using System.Text.Json.Serialization;

namespace Relias.ContentLibraryService.Domain.Course.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LessonFormatType
{
    Unknown = 0,
    Url = 1,
    Pdf = 2,
    Audio = 3,
    Video = 4,
    Scorm = 5,
    Aicc = 6
}
