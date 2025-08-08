using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class CourseContributor
{
    [Key]
    public int CourseContributorId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public int ContributorId { get; set; }

    public Contributor Contributor { get; set; } = null!;
}
