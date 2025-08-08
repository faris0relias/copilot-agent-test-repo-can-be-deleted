using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Relias.ContentLibraryService.Domain.Content;
using Relias.ContentLibraryService.Domain.Course;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    private const string NvarcharMax = "nvarchar(max)";
    private const string Nvarchar50 = "nvarchar(50)";

    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.CourseId);

        builder.HasOne<Content>()
               .WithOne()
               .HasForeignKey<Course>(c => c.ContentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.OrganizationId)
               .IsRequired();

        builder.Property(c => c.ContentCode)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Title)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(c => c.Description)
               .HasColumnType(NvarcharMax);

        builder.Property(c => c.BriefDescription)
               .HasMaxLength(140);

        builder.Property(c => c.StatusId)
               .IsRequired();

        // LanguageIds stored as JSON in nvarchar(max)
        builder.Property(c => c.LanguageIds)
               .HasColumnType(NvarcharMax)
               .HasConversion(
                   v => JsonConvert.SerializeObject(v), 
                   v => JsonConvert.DeserializeObject<List<Guid>>(v) ?? new List<Guid>());

        builder.Property(c => c.Created)
               .IsRequired()
               .HasColumnType("datetime2")
               .HasDefaultValueSql("GETDATE()");

        builder.Property(c => c.CreatedBy)
               .HasColumnType(Nvarchar50);

        builder.Property(c => c.LastModified)
               .HasColumnType("datetime2");

        builder.Property(c => c.LastModifiedBy)
               .HasColumnType(Nvarchar50);

        // new props for review

        builder.Property(c => c.IsRelias)
           .IsRequired()
           .HasDefaultValue(false);

        builder.Property(c => c.PublishDate)
               .HasColumnType("datetime2");

        builder.Property(c => c.PublishBy)
               .HasColumnType(Nvarchar50);

        builder.Property(c => c.NextReviewDate)
               .HasColumnType("datetime2");

        builder.Property(c => c.ArchiveDate)
               .HasColumnType("datetime2");

        builder.Property(c => c.ArchiveBy)
               .HasColumnType(Nvarchar50);

        builder.Property(c => c.TimeToCompleteInMinutes);

        builder.ToTable(nameof(Course));
    }
}
