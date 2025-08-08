using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class Disclosure
{
    [Key]
    public int DisclosureId { get; set; }

    [Required]
    public string Statement { get; set; } = null!;

    public bool Active { get; set; }

    public int QuickbaseRecordId { get; set; }
}
