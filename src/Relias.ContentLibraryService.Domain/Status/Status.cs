using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Status
{
    public class Status
    {
        [Required]
        public byte StatusId { get; set; }

        [Required]
        public string Name { get; set; } = null!;
    }
}