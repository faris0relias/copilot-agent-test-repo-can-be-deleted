using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class DisclosureConfiguration : IEntityTypeConfiguration<Disclosure>
{
    public void Configure(EntityTypeBuilder<Disclosure> builder)
    {
        builder.HasKey(d => d.DisclosureId);

        builder.Property(d => d.DisclosureId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(d => d.Statement)
               .IsRequired()
               .HasColumnType("nvarchar(max)");

        builder.Property(lo => lo.Active)
               .IsRequired();

        builder.Property(d => d.QuickbaseRecordId)
               .IsRequired();

        builder.ToTable(nameof(Disclosure));
    }
}
