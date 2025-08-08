using Relias.ContentLibraryService.Domain.Language;

namespace Relias.ContentLibraryService.App.Interfaces.Content;

public interface ILanguageRepository
{
    Task<IEnumerable<Language>> GetLanguagesAsync(CancellationToken cancellationToken);
}
