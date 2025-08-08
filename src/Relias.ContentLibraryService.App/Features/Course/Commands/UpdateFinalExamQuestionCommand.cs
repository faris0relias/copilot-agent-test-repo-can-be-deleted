using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands;

public static class UpdateFinalExamQuestionCommand
{
    public class Contract : IRequest<FinalExamDto>
    {
       
        public required Guid CourseId { get; init; }
        public required Guid ExamId { get; init; }
        public Guid QuestionId { get; init; }
        public required FinalExamQuestionPatchDto Updates { get; init; }

    }

    public class Handler(IFinalExamService finalExamService) : IRequestHandler<Contract,FinalExamDto>
    {
        public async Task<FinalExamDto> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await finalExamService.UpdateFinalExamQuestionAsync(contract.CourseId,contract.ExamId, contract.QuestionId, contract.Updates, cancellationToken);
        }
    }
}