using MediatR;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Interfaces.Content;

namespace Relias.ContentLibraryService.App.Features.Content.Queries;

public class GetContentByIdsQuery
{
    public class Contract : IRequest<IEnumerable<ContentInfoDto>>
    {
        public IEnumerable<Guid> ContentIds { get; init; } = [];
    }

    public class Handler(IContentService contentService) : IRequestHandler<Contract, IEnumerable<ContentInfoDto>>
    {
        public async Task<IEnumerable<ContentInfoDto>> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await contentService.GetContentByIdsAsync(contract.ContentIds, cancellationToken);
        }
    }
}
