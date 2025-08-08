using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Newtonsoft.Json;

using Relias.ContentLibraryService.Domain.Lesson;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.Property(t => t.LessonId)
                .IsRequired().HasDefaultValueSql("(newid())");

            builder.Property(t => t.Title)
                .IsRequired();

            builder.Property(t => t.Created)
              .IsRequired().HasColumnType(nameof(DateTime));

            builder.Property(t => t.Duration)
                 .IsRequired();

            builder.Property(e => e.TopicIds)
                  .HasColumnType("nvarchar(max)")
                  .IsRequired();
            builder.Property(e => e.TopicIds)
                .HasConversion(
               v => JsonConvert.SerializeObject(v),
               v => JsonConvert.DeserializeObject<List<Guid>>(v) ?? new List<Guid>());

            builder.HasOne(d => d.LessonType).WithMany(p => p.Lessons)
            .HasForeignKey(d => d.LessonTypeId)
            .HasConstraintName("FK_Lesson_LessonType");
            builder.Property(e => e.LastModified).HasColumnType(nameof(DateTime));
            builder.ToTable(nameof(Lesson));
        }
    }
}
