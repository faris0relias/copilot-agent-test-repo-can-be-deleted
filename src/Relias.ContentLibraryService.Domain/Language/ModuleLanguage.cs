using Relias.ContentLibraryService.Common;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Language
{
    public class ModuleLanguage : AuditableEntity
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public Guid LanguageId { get; set; }

        [Required]
        public Guid ModuleId { get; set; }

        public virtual Module.Module Module { get; set; } = null!;
        public virtual Language Language { get; set; } = null!;
    }
}