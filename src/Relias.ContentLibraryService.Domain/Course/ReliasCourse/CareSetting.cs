using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class CareSetting
{
    [Key]
    public int CareSettingId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Setting { get; set; } = null!;

    public int QuickbaseRecordId { get; set; }
}