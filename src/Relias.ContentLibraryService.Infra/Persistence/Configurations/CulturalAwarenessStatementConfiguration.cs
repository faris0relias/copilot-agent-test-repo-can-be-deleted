using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class CulturalAwarenessStatementConfiguration : IEntityTypeConfiguration<CulturalAwarenessStatement>
{
    public void Configure(EntityTypeBuilder<CulturalAwarenessStatement> builder)
    {
        builder.HasKey(d => d.CulturalAwarenessStatementId);

        builder.Property(d => d.CulturalAwarenessStatementId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(d => d.Statement)
               .IsRequired()
               .HasColumnType("nvarchar(max)");

        builder.ToTable(nameof(CulturalAwarenessStatement));
    }
}
