using Relias.ContentLibraryService.Common;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;
using Relias.ContentLibraryService.Domain.Language;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Relias.ContentLibraryService.Domain.Module
{
    public class Module : AuditableEntity
    {
        [Required]
        public required Guid ModuleId { get; set; }

        [Required]
        public required string ModuleName { get; set; }

        [Required]
        public required Guid ModuleTypeId { get; set; }

        [Required]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public short Version { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        public List<Guid> LanguageIds { get; set; }

        public virtual Organization.Organization Organization { get; set; } = null!;
        public virtual ModuleType ModuleType { get; set; } = null!;
        public virtual ICollection<ModuleLanguage> ModuleLanguages { get; set; } = [];

        [Required]
        public string BriefDescription { get; set; } = null!;

        [Required]
        public string ModuleCode { get; set; } = null!;

        [NotMapped]
        public virtual ICollection<TargetAudience> TargetAudiences { get; set; } = [];

        [NotMapped]
        public virtual ICollection<Setting.Setting> Settings { get; set; } = [];

        public DateTime? FirstPublishDate { get; set; }

        public DateTime? LastPublishDate { get; set; }

        public DateTime? ReviewDate { get; set; }
        public DateTime? ArchiveDate { get; set; }

        [Required]
        public byte StatusId { get; set; }

        public virtual Status.Status Status { get; set; }

        [Required]
        public List<Guid> TargetAudienceIds { get; set; } = new List<Guid>();

        [Required]
        public List<Guid> SettingIds { get; set; } = new List<Guid>();
    }
}