using Newtonsoft.Json.Linq;
using Relias.ContentLibraryService.Domain.Course.FinalExam;
using System.Text.Json;

namespace Relias.ContentLibraryService.App.Helpers;
public static class JsonConverterHelper
{   
    private static readonly JsonSerializerOptions _camelCaseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static object? ParsePropertyValue(string key, JToken propertyValue)
    {
        switch (key.ToLower())
        {
            case "id":
            case "minimumpercentagetopass":
            case "questionsdisplayedperexam":
                return propertyValue.ToObject<int?>();
            case "duration":
                return propertyValue.ToObject<Duration?>();
            case "finalexamquestions":
                return propertyValue.ToObject<List<FinalExamQuestion>?>();
            default:
                throw new InvalidOperationException($"Unsupported property key: {key}");
        }
    }
}