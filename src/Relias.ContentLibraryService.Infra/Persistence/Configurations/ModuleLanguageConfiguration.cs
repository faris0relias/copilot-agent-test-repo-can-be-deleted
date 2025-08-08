using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Relias.ContentLibraryService.Domain.Language;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class ModuleLanguageConfiguration : IEntityTypeConfiguration<ModuleLanguage>
    {
        public void Configure(EntityTypeBuilder<ModuleLanguage> builder)
        {
            builder.HasOne(d => d.Module).WithMany(p => p.ModuleLanguages)
               .HasForeignKey(d => d.ModuleId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK_ModuleLanguage_Module");
            builder.Property(t => t.Id)
                 .IsRequired().HasDefaultValueSql("(newid())");
            builder.Property(t => t.Created)
               .IsRequired().HasColumnType(nameof(DateTime));
            builder.Property(e => e.LastModified).HasColumnType(nameof(DateTime));
            builder.ToTable(nameof(ModuleLanguage));
        }
    }
}
