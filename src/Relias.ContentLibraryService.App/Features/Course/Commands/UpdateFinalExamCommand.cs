using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands;

public static class UpdateFinalExamCommand 
{
    public class Contract : IRequest<FinalExamDto>
    {
        public required Dictionary<string, dynamic> UpdatedValues { get; init; }
        public required FinalExamDto UpdatedFinalExam { get; init; }
        public required Guid CourseId { get; init; }
        public required int OrganizationId { get; init; }

    }

    public class Handler(IFinalExamService finalExamService) : IRequestHandler<Contract, FinalExamDto>
    {
        public async Task<FinalExamDto> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await finalExamService.UpdateFinalExamAsync(contract.CourseId, contract.OrganizationId, contract.UpdatedValues, cancellationToken);
        }
    }
}