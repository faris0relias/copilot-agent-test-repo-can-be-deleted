using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class CommercialProductDisclaimer
{
    [Key]
    public int CommercialProductDisclaimerId { get; set; }

    [Required]
    public string Disclaimer { get; set; } = null!;
}
