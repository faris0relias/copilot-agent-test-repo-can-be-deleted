using MediatR;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Queries.Learner;

public sealed class GetLearnerFinalExamQuery
{
    public class Contract : IRequest<Object?>
    {
        public Guid CourseId { get; init; }

        public required List<Guid> QuestionIds { get; init; }

        public bool IsCompleted { get; set; }
        public required string lang { get; init; }

    }

    public class Handler(IFinalExamService service) : IRequestHandler<Contract, Object?>
    {
        public async Task<Object?> Handle(Contract contract, CancellationToken cancellationToken)
        {
         return await service.GetQuestionsByIdsAsync(contract.CourseId, contract.QuestionIds, contract.IsCompleted, contract.lang, cancellationToken);   
        }
    }
}