using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Relias.ContentLibraryService.Domain.Organization;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class OrganizationLibraryConfiguration : IEntityTypeConfiguration<OrganizationLibrary>
    {
        public void Configure(EntityTypeBuilder<OrganizationLibrary> builder)
        {
            builder.HasOne(d => d.Organization).WithMany(p => p.OrganizationLibraries)
               .HasForeignKey(d => d.OrganizationId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK_OrganizationLibrary_Organization");
            builder.HasOne(d => d.Library).WithMany(p => p.OrganizationLibraries)
              .HasForeignKey(d => d.LibraryId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_OrganizationLibrary_Library");
            builder.Property(t => t.Id)
                 .IsRequired().HasDefaultValueSql("(newid())");
            builder.Property(t => t.Created)
               .IsRequired().HasColumnType(nameof(DateTime));
            builder.Property(e => e.LastModified).HasColumnType(nameof(DateTime));
            builder.ToTable(nameof(OrganizationLibrary));
        }
    }
}
