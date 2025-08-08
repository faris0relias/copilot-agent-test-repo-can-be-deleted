using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Content;
using Relias.ContentLibraryService.Domain.ContentType;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class ContentTypeConfiguration : IEntityTypeConfiguration<ContentType>
    {
        public void Configure(EntityTypeBuilder<ContentType> builder)
        {
            builder.HasKey(c => c.ContentTypeId);
            
            builder.Property("ContentTypeId").IsRequired();
            builder.Property("ContentTypeDescription").IsRequired();

            builder.HasMany<Content>()
                .WithOne(e => e.ContentType)
                .HasForeignKey(e => e.ContentTypeId);
        }
    }
}