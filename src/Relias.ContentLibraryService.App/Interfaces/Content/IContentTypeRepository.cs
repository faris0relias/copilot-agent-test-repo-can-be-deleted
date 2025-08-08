using Relias.ContentLibraryService.Domain.ContentType;

namespace Relias.ContentLibraryService.App.Interfaces.Content;

public interface IContentTypeRepository
{
    Task<IEnumerable<ContentType>> GetContentTypesAsync(CancellationToken cancellationToken);
}
