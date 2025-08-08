using Relias.ContentLibraryService.Common;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Organization
{
    public class Organization : AuditableEntity
    {
        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        public string? OrganizationName { get; set; }

        [Required]
        public int LegacyOrganizationId { get; set; }

        [Required]
        public string OrganizationCode { get; set; }

        public virtual ICollection<Module.Module> Modules { get; set; } = [];
        public virtual ICollection<Library.Library> Libraries { get; set; } = [];
        public virtual ICollection<OrganizationLibrary> OrganizationLibraries { get; set; } = [];
    }
}