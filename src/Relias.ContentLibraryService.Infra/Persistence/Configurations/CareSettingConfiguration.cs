using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class CareSettingConfiguration : IEntityTypeConfiguration<CareSetting>
{
    public void Configure(EntityTypeBuilder<CareSetting> builder)
    {
        builder.HasKey(cs => cs.CareSettingId);

        builder.Property(cs => cs.CareSettingId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(cs => cs.Setting)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(d => d.QuickbaseRecordId)
               .IsRequired();

        builder.ToTable(nameof(CareSetting));
    }
}
