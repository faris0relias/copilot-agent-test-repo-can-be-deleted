using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class CourseDisclosure
{
    [Key]
    public int CourseDisclosureId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public int DisclosureId { get; set; }

    public Disclosure Disclosure { get; set; } = null!;
}
