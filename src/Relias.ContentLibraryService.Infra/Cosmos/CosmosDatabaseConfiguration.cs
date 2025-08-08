using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Relias.ContentLibraryService.Common.Models;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Relias.ContentLibraryService.Infra.Cosmos;

[ExcludeFromCodeCoverage(Justification = "Startup wiring only; validated via integration tests, no business logic.")]
public static class CosmosDatabaseConfiguration
{
    public static IServiceCollection ConfigureCosmos(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var appSettings = configuration.GetRequiredSection(nameof(AppSettings)).Get<AppSettings>()!;
        services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
        services.AddOptions<AppSettings>(nameof(AppSettings));
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        jsonOptions.Converters.Add(new LearningObjectConverter());

        var cosmosClientBuilder = environment.IsDevelopment() || environment.EnvironmentName == "Local"
            ? new CosmosClientBuilder(appSettings.CosmosConnectionString)
            : new CosmosClientBuilder(appSettings.AccountEndpoint, new DefaultAzureCredential());

        cosmosClientBuilder.WithCustomSerializer(new SystemTextJsonCosmosSerializer(jsonOptions));

        var cosmosClient = cosmosClientBuilder.Build();

        var linqSerializer = new CosmosSystemTextJsonLinqSerializer(jsonOptions);

        var linqOptions = new CosmosLinqSerializerOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        };

        typeof(CosmosLinqSerializerOptions)
            .GetProperty("CosmosLinqSerializer", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            ?.SetValue(linqOptions, linqSerializer);

        services.AddSingleton(cosmosClient);
        services.AddSingleton(linqOptions);

        services.AddSingleton<ICosmosClientWrapper>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var options = sp.GetRequiredService<CosmosLinqSerializerOptions>();
            return new CosmosClientWrapper(client, appSettings.DatabaseId, options);
        });

        return services;
    }
}
