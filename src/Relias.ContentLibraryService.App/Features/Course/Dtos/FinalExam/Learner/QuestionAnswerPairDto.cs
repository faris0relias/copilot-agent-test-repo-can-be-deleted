using Relias.ContentLibraryService.Domain.Course.Enums;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;

public class QuestionAnswerPairDto
{
    public Guid QuestionId { get; set; }
    public List<Guid> CorrectOptionIds { get; set; } = [];
}

