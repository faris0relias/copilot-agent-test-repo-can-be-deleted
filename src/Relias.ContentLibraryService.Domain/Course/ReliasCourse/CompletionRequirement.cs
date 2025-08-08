using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class CompletionRequirement
{
    [Key]
    public int CompletionRequirementId { get; set; }

    [Required]
    public string Requirement { get; set; } = null!;
}
