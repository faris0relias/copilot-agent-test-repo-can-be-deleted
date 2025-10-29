using MediatR;
using Relias.ContentLibraryService.Common.DTO;

namespace Relias.ContentLibraryService.App.Features
{
    public class GetHealthStatusQuery : IRequest<HealthStatusDto>
    {
    }
}
