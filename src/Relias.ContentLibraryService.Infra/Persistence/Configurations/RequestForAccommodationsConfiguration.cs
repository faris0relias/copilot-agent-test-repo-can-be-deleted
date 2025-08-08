using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class RequestForAccommodationsConfiguration : IEntityTypeConfiguration<RequestForAccommodations>
{
    public void Configure(EntityTypeBuilder<RequestForAccommodations> builder)
    {
        builder.HasKey(d => d.RequestForAccommodationsId);

        builder.Property(d => d.RequestForAccommodationsId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(d => d.Request)
               .IsRequired()
               .HasColumnType("nvarchar(max)");

        builder.ToTable(nameof(RequestForAccommodations));
    }
}
