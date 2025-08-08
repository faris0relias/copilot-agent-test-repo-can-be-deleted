using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class CompletionRequirementConfiguration : IEntityTypeConfiguration<CompletionRequirement>
{
    public void Configure(EntityTypeBuilder<CompletionRequirement> builder)
    {
        builder.HasKey(d => d.CompletionRequirementId);

        builder.Property(d => d.CompletionRequirementId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(d => d.Requirement)
               .IsRequired()
               .HasColumnType("nvarchar(max)");

        builder.ToTable(nameof(CompletionRequirement));
    }
}
