namespace Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;

public class FinalExamQuestionPatchDto
{
    public FinalExamUpdateQuestionDto? QuestionUpdate { get; set; }
    public List<FinalExamQuestionUpdateOptionDto>? QuestionOptionsUpdate { get; set; } = [];
}
