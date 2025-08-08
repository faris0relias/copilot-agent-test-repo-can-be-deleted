using Relias.ContentLibraryService.Common;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Module
{
    public class ModuleCategory : AuditableEntity
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public required string Name { get; set; }
        public virtual ICollection<ModuleType> ModuleTypes { get; set; } = [];
    }
}