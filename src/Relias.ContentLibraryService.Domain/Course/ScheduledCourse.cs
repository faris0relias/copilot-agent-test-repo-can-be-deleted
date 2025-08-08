using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course;

public class ScheduledCourse
{
    [Key]
    public int ScheduledCourseId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public int StatusId { get; set; }

    public string? ScheduledBy { get; set; }

    [Required]
    public DateTime ScheduledDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
