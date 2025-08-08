using Relias.ContentLibraryService.Domain.Course.Enums;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;

public class FinalExamQuestionLearnerDto
{
    public required Guid QuestionId { get; set; }
    public required LocalizedStringDto QuestionText { get; set; }
    public QuestionType? QuestionType { get; set; }
    public List<FinalExamQuestionOptionsLearnerDto>? QuestionOptions { get; set; } = [];
}
