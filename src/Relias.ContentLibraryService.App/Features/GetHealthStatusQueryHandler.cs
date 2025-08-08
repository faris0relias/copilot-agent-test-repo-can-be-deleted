using MediatR;
using Relias.ContentLibraryService.Common.DTO;
using System.Threading;
using System.Threading.Tasks;

namespace Relias.ContentLibraryService.App.Features
{
    public class GetHealthStatusQueryHandler : IRequestHandler<GetHealthStatusQuery, HealthStatusDto>
    {
        public Task<HealthStatusDto> Handle(GetHealthStatusQuery request, CancellationToken cancellationToken)
        {
            // Simulate fetching health status
            return Task.FromResult(new HealthStatusDto { Status = "Healthy" });
        }
    }
}
