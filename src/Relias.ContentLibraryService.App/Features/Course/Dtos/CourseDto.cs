using Relias.ContentLibraryService.App.Features.Course.Dtos.ReliasCourse;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos;

public class CourseDto
{
    public Guid CourseId { get; init; }

    public Guid? ContentId { get; init; }

    public int OrganizationId { get; init; }

    public string? ContentCode { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? BriefDescription { get; init; }

    public List<Guid> LanguageIds { get; init; } = [];

    public int StatusId { get; init; }

    public required DateTime Created { get; init; }

    public string? CreatedBy { get; init; }

    public DateTime? LastModified { get; init; }

    public string? LastModifiedBy { get; init; }

    public bool IsRelias { get; init; } = false;

    public DateTime? PublishDate { get; init; }

    public string? PublishBy { get; init; }

    public DateTime? NextReviewDate { get; init; }

    public DateTime? ArchiveDate { get; init; }

    public string? ArchiveBy { get; init; }

    public int? TimeToCompleteInMinutes { get; init; }

    public ReliasCoursePropertiesDto? ReliasCourseProperties { get; init; }
}
