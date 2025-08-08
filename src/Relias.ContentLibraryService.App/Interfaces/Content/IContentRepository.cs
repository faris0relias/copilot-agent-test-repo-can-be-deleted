namespace Relias.ContentLibraryService.App.Interfaces.Content;

public interface IContentRepository
{
    Task<IEnumerable<Domain.Content.Content>> GetContentByContentIdsAsync(IEnumerable<Guid> contentIds, CancellationToken cancellationToken);
}
