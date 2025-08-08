using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class CourseTrainingTopicConfiguration : IEntityTypeConfiguration<CourseTrainingTopic>
{
    public void Configure(EntityTypeBuilder<CourseTrainingTopic> builder)
    {
        builder.HasKey(cc => cc.CourseTrainingTopicsId);

        builder.Property(cc => cc.CourseTrainingTopicsId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(cc => cc.CourseId)
               .IsRequired();

        builder.HasIndex(cc => new { cc.CourseId, cc.TrainingTopicId })
               .IsUnique();

        builder.HasOne(cc => cc.TrainingTopic)
               .WithMany()
               .HasForeignKey(cc => cc.TrainingTopicId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReliasCourseProperties>()
               .WithMany(rp => rp.TrainingTopics)
               .HasForeignKey(cc => cc.CourseId)
               .HasPrincipalKey(rp => rp.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(nameof(CourseTrainingTopic));
    }
}