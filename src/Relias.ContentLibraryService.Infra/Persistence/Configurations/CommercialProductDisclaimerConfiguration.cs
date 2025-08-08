using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class CommercialProductDisclaimerConfiguration : IEntityTypeConfiguration<CommercialProductDisclaimer>
{
    public void Configure(EntityTypeBuilder<CommercialProductDisclaimer> builder)
    {
        builder.HasKey(d => d.CommercialProductDisclaimerId);

        builder.Property(d => d.CommercialProductDisclaimerId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(d => d.Disclaimer)
               .IsRequired()
               .HasColumnType("nvarchar(max)");

        builder.ToTable(nameof(CommercialProductDisclaimer));
    }
}
