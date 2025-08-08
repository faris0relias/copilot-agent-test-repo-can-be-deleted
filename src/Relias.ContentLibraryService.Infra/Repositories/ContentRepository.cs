using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.App.Interfaces.Content;
using Relias.ContentLibraryService.Infra.Persistence;

namespace Relias.ContentLibraryService.Infra.Repositories;

public class ContentRepository(ApplicationDbContext dbContext) : IContentRepository
{
    public async Task<IEnumerable<Domain.Content.Content>> GetContentByContentIdsAsync(IEnumerable<Guid> contentIds, CancellationToken cancellationToken)
    {
        return await dbContext.Content.Where(c => contentIds.Contains(c.ContentId)).ToListAsync(cancellationToken);
    }
}

