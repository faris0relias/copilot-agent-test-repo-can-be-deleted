using Relias.ContentLibraryService.Common;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Organization
{
    public class OrganizationLibrary : AuditableEntity
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        public Guid LibraryId { get; set; }

        public bool IsReadOnly { get; set; }

        public virtual Organization Organization { get; set; } = null!;

        public virtual Library.Library Library { get; set; } = null!;
    }
}