using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Content;
using Relias.ContentLibraryService.Domain.Policy;
using Relias.ContentLibraryService.Domain.PolicyTag;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
    {
        public void Configure(EntityTypeBuilder<Policy> builder)
        {
            builder.HasKey(c => c.PolicyId);
            
            builder.HasOne<Content>()                
                .WithOne()                     
                .HasForeignKey<Policy>(p => p.ContentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Property(c => c.ContentId).IsRequired();
            builder.Property(p => p.PolicyId).IsRequired();
            builder.Property(o => o.OrgId).IsRequired();
            builder.Property(p => p.PolicyEventReceivedDate).IsRequired();
            builder.Property(p => p.PolicyPublishedDate).IsRequired();
            builder.Property(t => t.Title).IsRequired();
            builder.Property(t => t.Topic).IsRequired();
            builder.Property(s => s.Subtopic).IsRequired();
            builder.Property(d => d.Description).IsRequired(false);  
            builder.Property(i => i.IsArchived).IsRequired();
            
            builder.HasMany(p => p.Tags)
                .WithMany(t => t.Policies)
                .UsingEntity<Dictionary<string, object>>(
                    "PolicyTag",
                    j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                    j => j.HasOne<Policy>().WithMany().HasForeignKey("PolicyId"));
        }
    }
}