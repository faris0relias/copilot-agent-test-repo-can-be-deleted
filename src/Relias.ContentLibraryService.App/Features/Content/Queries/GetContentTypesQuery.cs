using MediatR;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Interfaces.Content;

namespace Relias.ContentLibraryService.App.Features.Content.Queries;

public class GetContentTypesQuery
{
    public class Contract : IRequest<IEnumerable<ContentTypeDto>> { }

    public class Handler(IContentTypeService contentTypeService)
        : IRequestHandler<Contract, IEnumerable<ContentTypeDto>>
    {
        public async Task<IEnumerable<ContentTypeDto>> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await contentTypeService.GetContentTypesAsync(cancellationToken);
        }
    }
}

