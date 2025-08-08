using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class TrainingTopicConfiguration : IEntityTypeConfiguration<TrainingTopic>
{
    public void Configure(EntityTypeBuilder<TrainingTopic> builder)
    {
        builder.HasKey(t => t.TrainingTopicId);

        builder.Property(t => t.TrainingTopicId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(t => t.Topic)
               .IsRequired()
               .HasColumnType("nvarchar(max)");

        builder.Property(t => t.Active)
               .IsRequired();

        builder.Property(d => d.QuickbaseRecordId)
               .IsRequired();

        builder.ToTable(nameof(TrainingTopic));
    }
}
