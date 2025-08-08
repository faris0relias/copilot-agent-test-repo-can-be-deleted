using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class LearningObjectiveConfiguration : IEntityTypeConfiguration<LearningObjective>
{
    public void Configure(EntityTypeBuilder<LearningObjective> builder)
    {
        builder.HasKey(lo => lo.LearningObjectiveId);

        builder.Property(lo => lo.LearningObjectiveId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(lo => lo.CourseId)
               .IsRequired();

        builder.Property(lo => lo.Active)
               .IsRequired();

        builder.Property(lo => lo.Objective)
               .IsRequired()
               .HasColumnType("nvarchar(max)");

        builder.Property(d => d.QuickbaseRecordId)
               .IsRequired();

        builder.HasOne<ReliasCourseProperties>()
               .WithMany(rcp => rcp.LearningObjectives)
               .HasForeignKey(lo => lo.CourseId)
               .HasPrincipalKey(rp => rp.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(nameof(LearningObjective));
    }
}
