using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class TargetAudienceConfiguration : IEntityTypeConfiguration<TargetAudience>
{
    public void Configure(EntityTypeBuilder<TargetAudience> builder)
    {
        builder.HasKey(t => t.TargetAudienceId);

        builder.Property(t => t.TargetAudienceId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(t => t.Audience)
               .HasColumnType("nvarchar(max)")
               .IsRequired();

        builder.Property(t => t.Active)
               .IsRequired();

        builder.Property(t => t.QuickbaseRecordId)
               .IsRequired();

        builder.ToTable(nameof(TargetAudience));
    }
}
