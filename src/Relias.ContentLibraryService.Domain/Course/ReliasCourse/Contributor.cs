using Relias.ContentLibraryService.Common;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class Contributor : AuditableEntity
{
    [Key]
    public int ContributorId { get; set; }

    public int? ContributorIdNum { get; set; }

    [MaxLength(255)]
    public string NameAndCredentials { get; set; } = null!;

    public string? ShortBio { get; set; }

    public string? DisclosureStatement { get; set; }
}