using Relias.ContentLibraryService.App.Features.Content.Dtos;

namespace Relias.ContentLibraryService.App.Interfaces.Content;

public interface IContentService
{
    Task<IEnumerable<ContentInfoDto>> GetContentByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
}
