using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

class CourseCareSettingConfiguration : IEntityTypeConfiguration<CourseCareSetting>
{
    public void Configure(EntityTypeBuilder<CourseCareSetting> builder)
    {
        builder.HasKey(cc => cc.CourseCareSettingId);

        builder.Property(cc => cc.CourseCareSettingId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(cc => cc.CourseId)
               .IsRequired();

        builder.HasIndex(cc => new { cc.CourseId, cc.CareSettingId })
               .IsUnique();

        builder.HasOne(cc => cc.CareSetting)
               .WithMany()
               .HasForeignKey(cc => cc.CareSettingId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReliasCourseProperties>()
               .WithMany(rp => rp.CareSettings)
               .HasForeignKey(ccs => ccs.CourseId)
               .HasPrincipalKey(rp => rp.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(nameof(CourseCareSetting));
    }
}
