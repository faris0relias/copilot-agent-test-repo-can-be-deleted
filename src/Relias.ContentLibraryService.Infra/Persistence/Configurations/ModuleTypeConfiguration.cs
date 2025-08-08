using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Relias.ContentLibraryService.Domain.Module;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class ModuleTypeConfiguration : IEntityTypeConfiguration<ModuleType>
    {
        public void Configure(EntityTypeBuilder<ModuleType> builder)
        {
            builder.ToTable(nameof(ModuleType));
            builder.Property(t => t.Name).IsRequired();
            builder.Property(t => t.Id).IsRequired().HasDefaultValueSql("(newid())");
            builder.Property(t => t.Created).IsRequired().HasColumnType(nameof(DateTime));
            builder.Property(e => e.LastModified).HasColumnType(nameof(DateTime));
            builder.HasOne(d => d.ModuleCategory).WithMany(p => p.ModuleTypes)
                .HasForeignKey(d => d.ModuleCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ModuleType_ModuleCategory");
        }
    }
}
