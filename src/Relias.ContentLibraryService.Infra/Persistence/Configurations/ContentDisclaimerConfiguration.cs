using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class ContentDisclaimerConfiguration : IEntityTypeConfiguration<ContentDisclaimer>
{
    public void Configure(EntityTypeBuilder<ContentDisclaimer> builder)
    {
        builder.HasKey(d => d.ContentDisclaimerId);

        builder.Property(d => d.ContentDisclaimerId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(d => d.Disclaimer)
               .IsRequired()
               .HasColumnType("nvarchar(max)");

        builder.ToTable(nameof(ContentDisclaimer));
    }
}
