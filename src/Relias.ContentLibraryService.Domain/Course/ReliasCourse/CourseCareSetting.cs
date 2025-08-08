using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class CourseCareSetting
{
    [Key]
    public int CourseCareSettingId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public int CareSettingId { get; set; }

    public CareSetting CareSetting { get; set; } = null!;
}
