namespace Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;

public class FinalExamQuestionOptionsDto
{
    public required Guid? OptionId { get; set; }
    public required LocalizedStringDto? OptionText { get; set; }
    public LocalizedStringDto? ResponseFeedback { get; set; }
    public bool? IsCorrect { get; set; }
}
