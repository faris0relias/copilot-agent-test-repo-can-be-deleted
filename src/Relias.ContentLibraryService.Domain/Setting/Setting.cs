using Relias.ContentLibraryService.Common;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Setting
{
    public class Setting : AuditableEntity
    {
        [Required]
        public Guid SettingId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public virtual ICollection<Module.Module> Modules { get; set; } = [];
    }
}