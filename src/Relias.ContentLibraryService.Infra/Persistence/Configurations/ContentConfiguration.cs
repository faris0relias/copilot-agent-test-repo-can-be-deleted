using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Content;
using Relias.ContentLibraryService.Domain.ContentType;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class ContentConfiguration : IEntityTypeConfiguration<Content>
    {
        public void Configure(EntityTypeBuilder<Content> builder)
        {
            builder.HasKey(c => c.ContentId);
            
            builder.Property("ContentId").IsRequired()
                .HasColumnType("uniqueidentifier");
            builder.Property("ContentTypeId").IsRequired()
                .HasColumnType("int");

            builder.HasOne<ContentType>()
                .WithMany()
                .HasForeignKey(c => c.ContentTypeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}