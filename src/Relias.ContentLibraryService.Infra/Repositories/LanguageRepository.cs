using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.App.Interfaces.Content;
using Relias.ContentLibraryService.Domain.Language;
using Relias.ContentLibraryService.Infra.Persistence;

namespace Relias.ContentLibraryService.Infra.Repositories;

public class LanguageRepository(ApplicationDbContext dbContext) : ILanguageRepository
{
    public async Task<IEnumerable<Language>> GetLanguagesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Languages.ToListAsync(cancellationToken);
    }
}
