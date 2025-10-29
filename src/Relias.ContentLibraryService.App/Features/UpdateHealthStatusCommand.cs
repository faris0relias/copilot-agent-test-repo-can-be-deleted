using MediatR;
using Relias.ContentLibraryService.Common.DTO;

namespace Relias.ContentLibraryService.App.Features
{
    public class UpdateHealthStatusCommand : IRequest<bool>
    {
        public HealthStatusDto HealthStatus { get; set; }
    }
}
