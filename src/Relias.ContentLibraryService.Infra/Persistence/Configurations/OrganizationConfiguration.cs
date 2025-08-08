using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Organization;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.Property(t => t.OrganizationName)
                .IsRequired();
            builder.Property(t => t.OrganizationCode)
                .IsRequired();
            builder.Property(t => t.LegacyOrganizationId)
               .IsRequired();
            builder.Property(t => t.OrganizationId)
                .IsRequired().HasDefaultValueSql("(newid())");
            builder.Property(t => t.Created)
               .IsRequired().HasColumnType(nameof(DateTime));
            builder.Property(e => e.LastModified).HasColumnType(nameof(DateTime));
            builder.ToTable(nameof(Organization));
        }
    }
}
