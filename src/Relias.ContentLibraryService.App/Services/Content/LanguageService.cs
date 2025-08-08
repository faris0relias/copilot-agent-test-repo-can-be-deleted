using AutoMapper;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Interfaces.Content;

namespace Relias.ContentLibraryService.App.Services.Content;

public class LanguageService(ILanguageRepository languageRepository, IMapper mapper) : ILanguageService
{
    public async Task<IEnumerable<LanguageDto>> GetLanguagesAsync(CancellationToken cancellationToken)
    {
        var languages = await languageRepository.GetLanguagesAsync(cancellationToken);

        return mapper.Map<IEnumerable<LanguageDto>>(languages);
    }
}
