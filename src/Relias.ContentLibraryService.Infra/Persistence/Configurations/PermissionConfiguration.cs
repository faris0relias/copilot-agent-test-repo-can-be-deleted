using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Permission;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            _ = builder.Property(t => t.Name)
                .IsRequired();
            _ = builder.Property(t => t.Id)
                .IsRequired().HasDefaultValueSql("(newid())");
            _ = builder.Property(t => t.RoleId)
               .IsRequired();
    
            _ = builder.ToTable(nameof(Permission));
            _ = builder.Property(e => e.Created).HasColumnType(nameof(DateTime));
            _ = builder.Property(e => e.LastModified).HasColumnType(nameof(DateTime));
        }
    }
}

