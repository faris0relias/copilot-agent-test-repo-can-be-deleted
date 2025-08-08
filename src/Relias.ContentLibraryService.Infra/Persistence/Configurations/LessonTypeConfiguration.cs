using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Relias.ContentLibraryService.Domain.LessonType;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class LessonTypeConfiguration : IEntityTypeConfiguration<LessonType>
    {
        public void Configure(EntityTypeBuilder<LessonType> builder)
        {
            builder.Property(t => t.LessonTypeId)
                .IsRequired().HasDefaultValueSql("(newid())");
            builder.Property(t => t.LessonTypeName)
               .IsRequired();
            builder.Property(t => t.Created)
            .IsRequired().HasColumnType(nameof(DateTime));
            builder.Property(e => e.LastModified).HasColumnType(nameof(DateTime));
            builder.ToTable(nameof(LessonType));
        }
    }
}
