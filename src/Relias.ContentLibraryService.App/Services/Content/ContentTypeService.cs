using AutoMapper;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Interfaces.Content;

namespace Relias.ContentLibraryService.App.Services.Content;

public class ContentTypeService(IContentTypeRepository repository, IMapper mapper) : IContentTypeService
{
    public async Task<IEnumerable<ContentTypeDto>> GetContentTypesAsync(CancellationToken cancellationToken)
    {
        var contentTypes = await repository.GetContentTypesAsync(cancellationToken);

        return mapper.Map<IEnumerable<ContentTypeDto>>(contentTypes);
    }
}
