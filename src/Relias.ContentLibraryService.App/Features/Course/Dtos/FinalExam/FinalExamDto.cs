using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;

public class FinalExamDto
{
    public Guid Id { get; init; }
    public int? MinimumPercentageToPass { get; init; }
    public int? QuestionsDisplayedPerExam { get; init; }
    public Duration? Duration { get; init; }
    public List<FinalExamQuestionDto>? FinalExamQuestions { get; set; } = [];
}
