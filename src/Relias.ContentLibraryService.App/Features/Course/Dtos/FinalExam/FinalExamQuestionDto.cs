using Relias.ContentLibraryService.Domain.Course.Enums;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;

public class FinalExamQuestionDto
{
    public required Guid QuestionId { get; set; }
    public required LocalizedStringDto QuestionText { get; set; }
    public QuestionType? QuestionType { get; set; }
    public List<FinalExamQuestionOptionsDto>? QuestionOptions { get; set; } = [];
}
