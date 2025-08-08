using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Module;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class ModuleCategoryConfiguration : IEntityTypeConfiguration<ModuleCategory>
    {
        public void Configure(EntityTypeBuilder<ModuleCategory> builder)
        {
            builder.Property(t => t.Name)
                .IsRequired();
            builder.Property(t => t.Id)
                .IsRequired().HasDefaultValueSql("(newid())");
            builder.Property(t => t.Created)
                .IsRequired().HasColumnType(nameof(DateTime));
            builder.Property(e => e.LastModified).HasColumnType(nameof(DateTime));
            builder.ToTable(nameof(ModuleCategory));
        }
    }
}
