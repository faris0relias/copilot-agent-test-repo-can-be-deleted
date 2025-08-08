using MediatR;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands;

// TODO: Not implemented, needs tests once implemented
public static class ArchiveCourseCommand
{
    public class Contract : IRequest 
    {
        public Guid CourseId { get; init; }
    }

    public class Handler(ICourseService service)
        : IRequestHandler<Contract>
    {
        public async Task Handle(Contract contract, CancellationToken cancellationToken)
        {
            // TODO: Add contract validator under /Features/Course/Validators/ArchiveCourseCommandContractValidator.cs

            // TODO: Implement the actual service method when available
            await service.ArchiveAsync(contract.CourseId, cancellationToken);
        }
    }
}
