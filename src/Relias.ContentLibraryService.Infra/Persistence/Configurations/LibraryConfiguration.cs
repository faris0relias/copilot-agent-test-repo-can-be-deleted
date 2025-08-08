using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Newtonsoft.Json;

using Relias.ContentLibraryService.Domain.Library;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class LibraryConfiguration : IEntityTypeConfiguration<Library>
    {
        public void Configure(EntityTypeBuilder<Library> builder)
        {
            builder.Property(t => t.LibraryName)
                .IsRequired();
            builder.Property(t => t.LibraryCode)
                .IsRequired();
            builder.Property(t => t.IsActive)
               .IsRequired();
            builder.Property(e => e.ModuleIds)
                  .HasColumnType("nvarchar(max)")
                  .IsRequired();
            builder.Property(e => e.ModuleIds)
                .HasConversion(
              v => JsonConvert.SerializeObject(v),
              v => JsonConvert.DeserializeObject<List<Guid>>(v) ?? new List<Guid>());
            builder.Property(t => t.LibraryId)
                .IsRequired().HasDefaultValueSql("(newid())");
            builder.HasOne(d => d.Organization).WithMany(p => p.Libraries)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Library_Organization");
            builder.Property(t => t.Created)
               .IsRequired().HasColumnType(nameof(DateTime));
            builder.Property(e => e.LastModified).HasColumnType(nameof(DateTime));
            builder.ToTable(nameof(Library));
        }
    }
}
