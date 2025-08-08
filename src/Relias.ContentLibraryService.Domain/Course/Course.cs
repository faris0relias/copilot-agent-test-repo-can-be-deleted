using Relias.ContentLibraryService.Common;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course;

public class Course : AuditableEntity
{
    [Key]
    public Guid CourseId { get; set; }

    [Required]
    public int OrganizationId { get; set; }

    [Required]
    public Guid? ContentId { get; set; }

    [Required, MaxLength(100)]
    public string ContentCode { get; set; } = null!;

    [Required, MaxLength(500)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [MaxLength(140)]
    public string? BriefDescription { get; set; } = string.Empty;

    public List<Guid>? LanguageIds { get; set; } = [];

    [Required]
    public byte StatusId { get; set; }

    public bool IsRelias { get; set; } = false;
    public DateTime? PublishDate { get; set; }
    public string? PublishBy { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public DateTime? ArchiveDate { get; set; }
    public string? ArchiveBy { get; set; }
    public int? TimeToCompleteInMinutes { get; set; }

    public ReliasCourseProperties? ReliasCourseProperties { get; set; }
}