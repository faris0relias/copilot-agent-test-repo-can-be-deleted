using MediatR;
using Relias.ContentLibraryService.Common.DTO;
using System.Threading;
using System.Threading.Tasks;

namespace Relias.ContentLibraryService.App.Features
{
    public class UpdateHealthStatusCommandHandler : IRequestHandler<UpdateHealthStatusCommand, bool>
    {
        public Task<bool> Handle(UpdateHealthStatusCommand request, CancellationToken cancellationToken)
        {
            // Simulate updating health status
            return Task.FromResult(true);
        }
    }
}
