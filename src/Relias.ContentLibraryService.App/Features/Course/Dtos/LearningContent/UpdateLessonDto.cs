using Relias.ContentLibraryService.Domain.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;

public class UpdateLessonDto : ILessonValidationModel
{
    public LearningObjectType LearningObjectType { get; set; }
    public Guid LearningObjectId { get; set; }
    public required string LessonType { get; set; }
    public required LocalizedStringDto Name { get; set; }
    public int  DurationMinutes { get; set; }
    public bool RequiredForCompletion { get; set; }
    public bool RequiresAudio { get; set; }
    public bool RequiresVideo { get; set; }
    public bool? OpensInNewTab { get; set; }   
    public string? ContentPath { get; set; }
    public string? FileName { get; set; }
    public string? FileSize { get; set; }
    public string? OrgId { get; set; }
    public bool DeleteFile { get; set; } = false;
    public LessonFormatType FormatType { get; set; }
}
