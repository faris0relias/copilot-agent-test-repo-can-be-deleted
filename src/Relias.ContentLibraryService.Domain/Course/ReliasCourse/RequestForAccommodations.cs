using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class RequestForAccommodations
{
    [Key]
    public int RequestForAccommodationsId { get; set; }

    [Required]
    public string Request { get; set; } = null!;
}
