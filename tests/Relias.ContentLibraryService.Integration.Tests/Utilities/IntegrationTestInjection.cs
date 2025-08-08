using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Relias.ContentLibraryService.Common.Constants;
using Relias.ContentLibraryService.Infra.Cosmos;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Infra.Persistence.Cosmos;
using System.Data;
using System.Reflection;

namespace Relias.ContentLibraryService.Integration.Tests.Utilities;

public class TestAzureClientFactory : IAzureClientFactory<BlobServiceClient>
{
    private readonly BlobServiceClient _blobServiceClient;

    public TestAzureClientFactory(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public BlobServiceClient CreateClient(string name)
    {
        return _blobServiceClient;
    }
}

public static class IntegrationTestInjection
{
    public static void AddTestAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(IntegrationTestAuthHandler.TestAuthScheme)
        .AddScheme<AuthenticationSchemeOptions, IntegrationTestAuthHandler>(
            IntegrationTestAuthHandler.TestAuthScheme, options =>
            {
                options.TimeProvider = TimeProvider.System;
            });
    }

    public static void AddTestCosmos(this IServiceCollection services, CosmosClientWrapper clientWrapper)
    {
        services.RemoveAll<ICosmosClientWrapper>();
        services.AddSingleton<ICosmosClientWrapper>(clientWrapper);
    }

    public static void AddTestSqlDatabase(this IServiceCollection services, string connectionString)
    {
        services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
        services.RemoveAll<IDbConnection>();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(Assembly.GetAssembly(typeof(ApplicationDbContext))?.FullName);
                }));

        services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));
    }

    public static void AddTestCosmosDatabase(this IServiceCollection services, CosmosClient cosmosClient)
    {
        services.RemoveAll<CosmosClient>();
        services.RemoveAll<ICosmosClientWrapper>();

        services.AddSingleton(cosmosClient);

        services.AddSingleton<ICosmosClientWrapper>(sp =>
        {
            var options = sp.GetRequiredService<CosmosLinqSerializerOptions>();
            return new CosmosClientWrapper(sp.GetRequiredService<CosmosClient>(), CosmosDbConstants.DatabaseName, options);
        });
    }    
    public static void AddTestAzurite(this IServiceCollection services, string connectionString)
    {
        services.RemoveAll<BlobServiceClient>();
        services.RemoveAll<IAzureClientFactory<BlobServiceClient>>();

        var blobServiceClient = new BlobServiceClient(connectionString);
        services.AddSingleton(blobServiceClient);

        services.AddSingleton<IAzureClientFactory<BlobServiceClient>>(new TestAzureClientFactory(blobServiceClient));
        
        services.AddScoped<Relias.ContentLibraryService.App.Interfaces.IMainBlobStorageRepository, 
                          Relias.ContentLibraryService.Infra.Repositories.MainBlobStorageRepository>();
    }
}

