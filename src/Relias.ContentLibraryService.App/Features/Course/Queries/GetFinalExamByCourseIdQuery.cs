using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Queries;

public sealed class GetFinalExamByCourseIdQuery
{
    public class Contract : IRequest<FinalExamDto?>
    {
        public Guid CourseId { get; init; }
        public required string lang { get; init; }
    }

    public class Handler(IFinalExamService service) : IRequestHandler<Contract, FinalExamDto?>
    {
        public async Task<FinalExamDto?> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await service.GetFinalExamByCourseIdAsync(contract.CourseId, contract.lang, cancellationToken);
        }
    }
}