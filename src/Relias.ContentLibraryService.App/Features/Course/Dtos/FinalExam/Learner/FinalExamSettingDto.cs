using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;

public class FinalExamSettingDto
{
    public Guid Id { get; init; }
    public int MinimumPercentageToPass { get; set; }
    public int QuestionsDisplayedPerExam { get; set; }
    public required Duration Duration { get; init; } 
}
