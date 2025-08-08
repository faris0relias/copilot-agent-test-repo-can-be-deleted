using Relias.ContentLibraryService.App.Features.Content.Dtos;

namespace Relias.ContentLibraryService.App.Interfaces.Content;

public interface IContentTypeService
{
    Task<IEnumerable<ContentTypeDto>> GetContentTypesAsync(CancellationToken cancellationToken);
}
