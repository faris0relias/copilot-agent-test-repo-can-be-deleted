using Relias.ContentLibraryService.Common;
using Relias.ContentLibraryService.Domain.Organization;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Library
{
    public class Library : AuditableEntity
    {
        [Required]
        public Guid LibraryId { get; set; }

        [Required]
        public string? LibraryCode { get; set; }

        [Required]
        public string? LibraryName { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public List<Guid> ModuleIds { get; set; }

        public virtual Organization.Organization Organization { get; set; } = null!;

        public virtual ICollection<OrganizationLibrary> OrganizationLibraries { get; set; } = [];
    }
}