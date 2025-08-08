using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class ScheduledCourseConfiguration : IEntityTypeConfiguration<ScheduledCourse>
{
    public void Configure(EntityTypeBuilder<ScheduledCourse> builder)
    {
        builder.HasKey(s => s.ScheduledCourseId);

        builder.Property(s => s.CourseId)
               .IsRequired();

        builder.Property(s => s.StatusId)
               .IsRequired();

        builder.Property(s => s.ScheduledDate)
               .IsRequired();

        builder.Property(c => c.ScheduledBy)
               .HasColumnType("nvarchar(50)");

        builder.Property(s => s.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne<Course>()
               .WithMany()
               .HasForeignKey(s => s.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.CourseId, s.StatusId })
               .IsUnique();

        builder.ToTable(nameof(ScheduledCourse));
    }
}
