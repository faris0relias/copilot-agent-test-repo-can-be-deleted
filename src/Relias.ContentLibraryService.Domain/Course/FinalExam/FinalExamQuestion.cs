using Relias.ContentLibraryService.Domain.Course.Enums;

namespace Relias.ContentLibraryService.Domain.Course.FinalExam;

public class FinalExamQuestion
{
    public required Guid QuestionId { get; set; }
    public required LocalizedString QuestionText { get; set; }
    public QuestionType? QuestionType { get; set; }
    public List<FinalExamQuestionOptions>? QuestionOptions { get; set; } = [];
}
