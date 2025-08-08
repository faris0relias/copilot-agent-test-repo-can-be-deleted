using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class LearningObjective
{
    [Key]
    public int LearningObjectiveId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    public bool Active { get; set; }

    [Required]
    public string Objective { get; set; } = null!;

    public int QuickbaseRecordId { get; set; }
}
