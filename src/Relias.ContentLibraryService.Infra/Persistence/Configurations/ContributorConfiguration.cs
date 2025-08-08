using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations;

public class ContributorConfiguration : IEntityTypeConfiguration<Contributor>
{
    private const string NvarcharMax = "nvarchar(max)";

    public void Configure(EntityTypeBuilder<Contributor> builder)
    {
        builder.HasKey(c => c.ContributorId);

        builder.Property(c => c.ContributorId)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(c => c.ContributorIdNum);

        builder.Property(c => c.NameAndCredentials)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(c => c.ShortBio)
               .HasColumnType(NvarcharMax);

        builder.Property(c => c.DisclosureStatement)
               .HasColumnType(NvarcharMax);

        builder.ToTable(nameof(Contributor));
    }
}
