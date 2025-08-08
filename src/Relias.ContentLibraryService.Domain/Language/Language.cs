using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Language;

public class Language
{
    [Required]
    public Guid LanguageId { get; set; }

    [Required]
    public string? Code { get; set; }

    [Required]
    public string? Name { get; set; }
}