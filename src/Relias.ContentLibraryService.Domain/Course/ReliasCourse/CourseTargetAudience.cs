using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class CourseTargetAudience
{
    [Key]
    public int CourseTargetAudienceId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public int TargetAudienceId { get; set; }

    public TargetAudience TargetAudience { get; set; } = null!;
}
