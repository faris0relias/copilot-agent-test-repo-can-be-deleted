using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.App.Interfaces.Content;
using Relias.ContentLibraryService.Domain.ContentType;
using Relias.ContentLibraryService.Infra.Persistence;

namespace Relias.ContentLibraryService.Infra.Repositories;

public class ContentTypeRepository(ApplicationDbContext dbContext) : IContentTypeRepository
{
    public async Task<IEnumerable<ContentType>> GetContentTypesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ContentType.ToListAsync(cancellationToken);
    }
}
