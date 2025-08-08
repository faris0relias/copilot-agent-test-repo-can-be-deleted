using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class ContentDisclaimer
{
    [Key]
    public int ContentDisclaimerId { get; set; }

    [Required]
    public string Disclaimer { get; set; } = null!;
}
