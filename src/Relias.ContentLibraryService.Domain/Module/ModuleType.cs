using Relias.ContentLibraryService.Common;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Module
{
    public class ModuleType : AuditableEntity
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public Guid ModuleCategoryId { get; set; }

        public string? Description { get; set; }

        public virtual ModuleCategory ModuleCategory { get; set; } = null!;

        public virtual ICollection<Module> Modules { get; set; } = [];
    }
}