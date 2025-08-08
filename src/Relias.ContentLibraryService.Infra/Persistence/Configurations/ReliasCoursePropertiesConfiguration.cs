using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class ReliasCoursePropertiesConfiguration : IEntityTypeConfiguration<ReliasCourseProperties>
{
    private const string NvarcharMax = "nvarchar(max)";

    public void Configure(EntityTypeBuilder<ReliasCourseProperties> builder)
    {
        builder.HasKey(p => p.ReliasCoursePropertiesId);

        builder.Property(p => p.ReliasCoursePropertiesId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(p => p.CourseId)
               .IsRequired();

        builder.HasOne<Course>()
               .WithOne(c => c.ReliasCourseProperties)
               .HasForeignKey<ReliasCourseProperties>(p => p.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.CommercialProductDisclaimer)
               .WithMany()
               .HasForeignKey(p => p.CommercialProductDisclaimerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.CommercialProductDisclaimerId)
               .IsRequired(false);

        builder.HasOne(p => p.CompletionRequirement)
               .WithMany()
               .HasForeignKey(p => p.CompletionRequirementId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.CompletionRequirementId)
               .IsRequired(false);

        builder.HasOne(p => p.ContentDisclaimer)
               .WithMany()
               .HasForeignKey(p => p.ContentDisclaimerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.ContentDisclaimerId)
               .IsRequired(false);

        builder.HasOne(p => p.CulturalAwarenessStatement)
               .WithMany()
               .HasForeignKey(p => p.CulturalAwarenessStatementId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.CulturalAwarenessStatementId)
               .IsRequired(false);

        builder.HasOne(p => p.RequestForAccommodations)
               .WithMany()
               .HasForeignKey(p => p.RequestForAccommodationsId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.RequestForAccommodationsId)
               .IsRequired(false);

        builder.Property(p => p.Outline).HasColumnType(NvarcharMax);

        builder.Property(p => p.Disclosure).HasColumnType(NvarcharMax);

        builder.ToTable(nameof(ReliasCourseProperties));
    }
}
