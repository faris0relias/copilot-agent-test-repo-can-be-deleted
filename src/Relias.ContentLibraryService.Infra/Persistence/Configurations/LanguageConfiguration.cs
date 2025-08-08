using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Relias.ContentLibraryService.Domain.Language;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class LanguageConfiguration : IEntityTypeConfiguration<Language>
    {
        public void Configure(EntityTypeBuilder<Language> builder)
        {
            builder.Property(t => t.Name)
                .IsRequired();

            builder.Property(t => t.Code)
                .IsRequired();

            builder.Property(t => t.LanguageId)
                .IsRequired().HasDefaultValueSql("(newid())");

            builder.ToTable(nameof(Language));
        }
    }
}
