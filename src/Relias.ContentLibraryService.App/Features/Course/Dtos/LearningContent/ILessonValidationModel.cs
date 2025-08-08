using Relias.ContentLibraryService.Domain.Course.Enums;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;

public interface ILessonValidationModel
{
    public string LessonType { get; set; }
    public LocalizedStringDto Name { get; set; }
    public int DurationMinutes { get; set; }
    public bool RequiredForCompletion { get; set; }
    public bool RequiresAudio { get; set; }
    public bool RequiresVideo { get; set; }
    public bool? OpensInNewTab { get; set; }
    public string? ContentPath { get; set; }
    public string? FileName { get; set; }
    public LessonFormatType FormatType { get; set; }
}
