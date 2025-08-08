using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;

// TODO: delete this file after updating BDD tests to use JsonPatch
public class LearningContentPatchDto
{
    public SectionUpdateDto? SectionUpdate { get; set; }
    public LearningObjectUpdateDto? LearningObjectUpdate { get; set; }
}

public enum PatchAction
{
    Add,
    Remove,
    Replace
}

public class SectionUpdateDto
{
    public required PatchAction Action { get; set; }
    public Guid SectionId { get; set; }
    public LocalizedStringDto? Name { get; set; }
}

public class LearningObjectUpdateDto
{
    public required PatchAction Action { get; set; }
    public required Guid SectionId { get; set; }
    public Guid LearningObjectId { get; set; }
    public LearningObjectType LearningObjectType { get; set; }

    public LessonUpdateDto? Lesson { get; set; }
}

public class LessonUpdateDto
{
    public Guid LearningObjectId { get; set; }
    public string? LessonType { get; set; }
    public LocalizedStringDto? Name { get; set; }
    public int? DurationMinutes { get; set; }
    public string? FormatType { get; set; }
    public bool? RequiredForCompletion { get; set; }
    public bool? RequiresAudio { get; set; }
    public bool? RequiresVideo { get; set; }
    public bool? OpensInNewTab { get; set; }
    public string? ContentPath { get; set; }
    public string? FileName { get; set; }
    public string? FileSize { get; set; }
}