using MediatR;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands;

public static class DeleteFinalExamCommand
{
    public class Contract(Guid courseId) : IRequest
    {
        public Guid CourseId { get; } = courseId;
    }

    public class Handler(IFinalExamService finalExamService) : IRequestHandler<Contract>
    {
        public async Task Handle(Contract contract, CancellationToken cancellationToken)
        {
            await finalExamService.DeleteFinalExamAsync(contract.CourseId, cancellationToken);
        }
    }
}
