using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relias.ContentLibraryService.Domain.Setting;

namespace Relias.ContentLibraryService.Infra.Persistence.Configurations
{
    public class SettingConfiguration : IEntityTypeConfiguration<Setting>
    {
        public void Configure(EntityTypeBuilder<Setting> builder)
        {
            builder.Property(s => s.SettingId)
                .IsRequired().HasDefaultValueSql("(newid())");

            builder.Property(s => s.Name)
                .IsRequired();
            builder.Ignore(m => m.Modules);

            builder.ToTable(nameof(Setting));
        }
    }
}
