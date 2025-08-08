using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Status;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class StatusConfiguration : IEntityTypeConfiguration<Status>
    {
        public void Configure(EntityTypeBuilder<Status> builder)
        {
            builder.Property(s => s.StatusId)
               .IsRequired();

            builder.Property(s => s.Name)
                .IsRequired();

            builder.ToTable(nameof(Status));
        }
    }
}
