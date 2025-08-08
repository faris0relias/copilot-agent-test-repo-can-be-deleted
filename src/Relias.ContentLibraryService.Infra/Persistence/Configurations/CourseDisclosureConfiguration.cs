using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class CourseDisclosureConfiguration : IEntityTypeConfiguration<CourseDisclosure>
{
    public void Configure(EntityTypeBuilder<CourseDisclosure> builder)
    {
        builder.HasKey(cc => cc.CourseDisclosureId);

        builder.Property(cc => cc.CourseDisclosureId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(cc => cc.CourseId)
               .IsRequired();

        builder.HasIndex(cc => new { cc.CourseId, cc.DisclosureId })
               .IsUnique();

        builder.HasOne(cc => cc.Disclosure)
               .WithMany()
               .HasForeignKey(cc => cc.DisclosureId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReliasCourseProperties>()
               .WithMany(rp => rp.Disclosures)
               .HasForeignKey(ccs => ccs.CourseId)
               .HasPrincipalKey(rp => rp.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(nameof(CourseDisclosure));
    }
}
