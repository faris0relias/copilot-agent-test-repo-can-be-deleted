using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Certificate;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
    {
        public void Configure(EntityTypeBuilder<Certificate> builder)
        {
            builder.Property(t => t.CertificateId)
                .IsRequired().HasDefaultValueSql("(newid())");
            builder.Property(t => t.Type)
               .IsRequired();

            builder.Property(t => t.Title)
               .IsRequired();
            builder.Property(t => t.Description)
               .IsRequired();
            builder.Property(t => t.Board)
               .IsRequired();
            builder.Property(c => c.CreditHours)
                   .IsRequired();
            builder.Property(c => c.StartDate)
                .IsRequired();
            builder.Property(c => c.ExpirationDate)
                .IsRequired();

            builder.ToTable(nameof(Certificate));
        }
    }
}
