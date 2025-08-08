using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands;

public class CreateFinalExamCommand
{
    public class Contract : IRequest<FinalExamDto>
    {
        public required Guid CourseId { get; init; }
        public required int OrganizationId { get; init; }
    }

    public class Handler(IFinalExamService finalExamService)
    : IRequestHandler<Contract, FinalExamDto>
    {
        public async Task<FinalExamDto> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await finalExamService.CreateFinalExamAsync(contract.CourseId, contract.OrganizationId, cancellationToken);
        }
    }

}
