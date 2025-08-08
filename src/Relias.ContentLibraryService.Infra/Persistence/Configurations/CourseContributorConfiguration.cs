using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class CourseContributorConfiguration : IEntityTypeConfiguration<CourseContributor>
{
    public void Configure(EntityTypeBuilder<CourseContributor> builder)
    {
        builder.HasKey(cc => cc.CourseContributorId);

        builder.Property(cc => cc.CourseContributorId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(cc => cc.CourseId)
               .IsRequired();

        builder.HasIndex(cc => new { cc.CourseId, cc.ContributorId })
               .IsUnique();

        builder.HasOne(cc => cc.Contributor)
               .WithMany()
               .HasForeignKey(cc => cc.ContributorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReliasCourseProperties>()
               .WithMany(rp => rp.Contributors)
               .HasForeignKey(cc => cc.CourseId)
               .HasPrincipalKey(rp => rp.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(nameof(CourseContributor));
    }
}
