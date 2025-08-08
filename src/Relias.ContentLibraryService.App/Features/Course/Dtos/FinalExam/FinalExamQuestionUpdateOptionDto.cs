using Relias.ContentLibraryService.App.Features.Course.Enums;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;

public class FinalExamQuestionUpdateOptionDto
{
    public required PatchAction Action { get; set; }
    public Guid OptionId { get; set; }
    public LocalizedStringDto? OptionText { get; set; }
    public LocalizedStringDto? ResponseFeedback { get; set; }
    public bool? IsCorrect { get; set; }
}
