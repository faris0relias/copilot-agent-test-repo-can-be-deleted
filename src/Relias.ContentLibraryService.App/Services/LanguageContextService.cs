using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Services;

public class LanguageContextService : ILanguageContextService
{
    private string _languageCode = "en"; // default fallback

    public string GetCurrentLanguage() => _languageCode;

    public void SetCurrentLanguage(string language)
    {
        if (!string.IsNullOrWhiteSpace(language))
        {
            _languageCode = language.ToLower();
        }
    }
}
