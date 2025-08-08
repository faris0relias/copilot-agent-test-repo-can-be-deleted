namespace Relias.ContentLibraryService.Domain.Course.FinalExam;

public class FinalExamQuestionOptions
{
    public required Guid? OptionId { get; set; }
    public required LocalizedString? OptionText { get; set; }
    public LocalizedString? ResponseFeedback { get; set; }
    public bool? IsCorrect { get; set; }
}
