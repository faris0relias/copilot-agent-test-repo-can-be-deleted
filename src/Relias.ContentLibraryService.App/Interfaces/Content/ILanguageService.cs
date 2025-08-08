using Relias.ContentLibraryService.App.Features.Content.Dtos;

namespace Relias.ContentLibraryService.App.Interfaces.Content;

public interface ILanguageService
{
    Task<IEnumerable<LanguageDto>> GetLanguagesAsync(CancellationToken cancellationToken);
}
