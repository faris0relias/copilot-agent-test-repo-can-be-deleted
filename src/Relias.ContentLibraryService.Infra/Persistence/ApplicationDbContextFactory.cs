using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Relias.ContentLibraryService.Infra.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration["SqlServerConnectionString"] ?? 
                               "Server=.;Database=content-library-service;Trusted_Connection=True;TrustServerCertificate=True;";
        
        var migrationsAssembly = typeof(ApplicationDbContextFactory).Assembly.FullName;

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}