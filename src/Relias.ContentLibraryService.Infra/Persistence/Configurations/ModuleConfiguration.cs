using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Relias.ContentLibraryService.Domain.Module;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.Property(t => t.ModuleName)
                .IsRequired();
            builder.Property(t => t.Version)
               .IsRequired();
            builder.Property(t => t.Title)
              .IsRequired();
            builder.Property(e => e.LanguageIds)
                 .HasColumnType("nvarchar(max)")
                 .IsRequired();

            builder.Property(e => e.LanguageIds)
                  .HasConversion(
                      v => JsonConvert.SerializeObject(v),
                      v => JsonConvert.DeserializeObject<List<Guid>>(v) ?? new List<Guid>());
            builder.Property(t => t.ModuleId)
                .IsRequired().HasDefaultValueSql("(newid())");
            builder.HasOne(d => d.Organization).WithMany(p => p.Modules)
               .HasForeignKey(d => d.OrganizationId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK_Module_Organization");
            builder.HasOne(d => d.ModuleType).WithMany(p => p.Modules)
               .HasForeignKey(d => d.ModuleTypeId)
               .HasConstraintName("FK_Module_ModuleType");
            builder.Property(t => t.Created)
               .IsRequired().HasColumnType(nameof(DateTime));
            builder.Property(e => e.LastModified).HasColumnType(nameof(DateTime));

            builder.Property(m => m.BriefDescription)
               .IsRequired();
            builder.Property(m => m.ModuleCode)
                .IsRequired();
            builder.Property(m => m.FirstPublishDate);
            builder.Property(m => m.LastPublishDate);
            builder.Property(m => m.ReviewDate);
            builder.Property(m => m.ArchiveDate);

            builder.Property(e => e.TargetAudienceIds)
             .HasColumnType("nvarchar(max)")
             .IsRequired();

            builder.Property(e => e.TargetAudienceIds)
                 .HasConversion(
                     v => JsonConvert.SerializeObject(v),
                     v => JsonConvert.DeserializeObject<List<Guid>>(v) ?? new List<Guid>());

            builder.Property(e => e.SettingIds)
              .HasColumnType("nvarchar(max)")
              .IsRequired();

            builder.Property(e => e.SettingIds)
                 .HasConversion(
                     v => JsonConvert.SerializeObject(v),
                     v => JsonConvert.DeserializeObject<List<Guid>>(v) ?? new List<Guid>());

            builder.Ignore(m => m.TargetAudiences);
            builder.Ignore(m => m.Settings);

            builder.ToTable(nameof(Module));
        }
    }
}
