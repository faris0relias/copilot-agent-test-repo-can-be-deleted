using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class CulturalAwarenessStatement
{
    [Key]
    public int CulturalAwarenessStatementId { get; set; }

    [Required]
    public string Statement { get; set; } = null!;
}
