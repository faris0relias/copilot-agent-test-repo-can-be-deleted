using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;

public class FinalExamSettingWithAnswerDto
{
    public Guid Id { get; init; }
    public int MinimumPercentageToPass { get; set; } = 0;
    public int QuestionsDisplayedPerExam { get; set; }
    public required Duration Duration { get; init; } 
    public List<QuestionAnswerPairDto> QuestionAnswerPairs { get; set; } = [];
}
