using Relias.ContentLibraryService.App.Features.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.Enums;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam
{
    public class FinalExamUpdateQuestionDto
    {
        public required PatchAction Action { get; set; }
        public Guid QuestionId { get; set; }
        public LocalizedStringDto? QuestionText { get; set; }
        public QuestionType? QuestionType { get; set; }
    }
}
