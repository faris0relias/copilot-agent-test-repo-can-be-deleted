using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class CourseTargetAudienceConfiguration : IEntityTypeConfiguration<CourseTargetAudience>
{
    public void Configure(EntityTypeBuilder<CourseTargetAudience> builder)
    {
        builder.HasKey(cc => cc.CourseTargetAudienceId);

        builder.Property(cc => cc.CourseTargetAudienceId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(cc => cc.CourseId)
               .IsRequired();

        builder.HasIndex(cc => new { cc.CourseId, cc.TargetAudienceId })
               .IsUnique();

        builder.HasOne(cc => cc.TargetAudience)
               .WithMany()
               .HasForeignKey(cc => cc.TargetAudienceId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReliasCourseProperties>()
               .WithMany(rp => rp.TargetAudiences)
               .HasForeignKey(cc => cc.CourseId)
               .HasPrincipalKey(rp => rp.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(nameof(CourseTargetAudience));
    }
}
