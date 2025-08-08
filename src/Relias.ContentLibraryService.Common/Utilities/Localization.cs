namespace Relias.ContentLibraryService.Common.Utilities;

public static class Localization
{
    public static TDto GetLocalizedText<TDto, TLocalized>(TLocalized? localized, string lang)
    where TDto : new()
    where TLocalized : class
    {
        TDto dto = new();

        if (localized == null) return dto;

        var langProperty = lang?.ToLowerInvariant() switch
        {
            "en" => "En",
            "fr" => "Fr",
            _ => "En"
        };

        var localizedProp = typeof(TLocalized).GetProperty(langProperty);
        var dtoProp = typeof(TDto).GetProperty(langProperty);

        if (localizedProp != null && dtoProp != null)
        {
            var value = localizedProp.GetValue(localized);
            dtoProp.SetValue(dto, value);
        }

        return dto;
    }

}
