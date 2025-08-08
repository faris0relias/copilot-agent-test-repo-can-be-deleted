namespace Relias.ContentLibraryService.App.Interfaces;

public interface ILanguageContextService
{
    string GetCurrentLanguage();              
    void SetCurrentLanguage(string language); 
}
