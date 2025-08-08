using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Relias.ContentLibraryService.Common.Constants;
using Relias.ContentLibraryService.Common.Models;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Relias.ContentLibraryService.Infra.Storage;

[ExcludeFromCodeCoverage]
public static class BlobStorageConfiguration
{
   
    public static IServiceCollection ConfigureBlobStorage(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        var appSettings = configuration.GetRequiredSection(nameof(AppSettings)).Get<AppSettings>()!;
        services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
        services.AddOptions<AppSettings>(nameof(AppSettings));

        if (environment.IsDevelopment() || environment.EnvironmentName == "Local")
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder
                    .AddBlobServiceClient(configuration.GetSection("LocalBlobStorageConnectionString"))
                    .WithName(AzureSdkClientConstants.MainBlobStorage);
            });
        }
        else
        {
            services.AddAzureClients(clientBuilder =>
            {
                Uri storageUri = new (appSettings.StorageAccount);

                clientBuilder.AddBlobServiceClient(storageUri)
                    .WithName(AzureSdkClientConstants.MainBlobStorage)
                    .WithCredential(new DefaultAzureCredential());
            });
        }
        
        return services;
    }
}
