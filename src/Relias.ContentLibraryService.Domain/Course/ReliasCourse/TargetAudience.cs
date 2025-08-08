using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class TargetAudience
{
    [Key]
    public int TargetAudienceId { get; set; }

    [Required]
    public string Audience { get; set; } = null!;

    public bool Active { get; set; }

    public int QuickbaseRecordId { get; set; }
}