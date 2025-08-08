using Azure.Identity;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.App.Services;
using Relias.ContentLibraryService.Common.Logging;
using Relias.ContentLibraryService.Common.Services;
using Relias.ContentLibraryService.Function.Function;
using Relias.ContentLibraryService.Infra.Cosmos;
using Relias.ContentLibraryService.Infra.Repositories;
using Relias.ContentLibraryService.Infra.Storage;
using Serilog;
using Serilog.Events;

var host = new HostBuilder().ConfigureFunctionsWebApplication()
    .ConfigureLogging(logging =>
    {
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule? defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
    
            if (defaultRule != null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    })
    .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.With(new CorrelationPropertyEnricher())
#if DEBUG
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3}] {Username} {Message:lj}{NewLine}{Exception}")
#endif
            // .WriteTo.ApplicationInsights(
            //     services.GetRequiredService<TelemetryConfiguration>(),
            //     TelemetryConverter.Traces)
        )
    .ConfigureHostConfiguration(cfgBuilder => cfgBuilder.AddEnvironmentVariables())
    .ConfigureAppConfiguration((builderContext, configurationBuilder) =>
    {
        const string PrimaryAppConfigurationEndpointKey = "APPCONFIGENDPOINTURL_PRIMARY";
        const string SecondaryAppConfigurationEndpointKey = "APPCONFIGENDPOINTURL_SECONDARY";

        // for non-local environments, use azure app config
        if (!builderContext.HostingEnvironment.IsEnvironment("Local") && !builderContext.HostingEnvironment.IsDevelopment())
        {
            var appConfigEndpoints = new List<Uri> 
            {
                new(builderContext.Configuration[PrimaryAppConfigurationEndpointKey] ?? throw new KeyNotFoundException(PrimaryAppConfigurationEndpointKey)),
                new(builderContext.Configuration[SecondaryAppConfigurationEndpointKey] ?? throw new KeyNotFoundException(SecondaryAppConfigurationEndpointKey))
            };

            configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options.Connect(appConfigEndpoints, new DefaultAzureCredential(new DefaultAzureCredentialOptions 
                {
                    ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
                }))
                .ConfigureKeyVault(static keyVaultOptions => keyVaultOptions.SetCredential(
                    new DefaultAzureCredential(new DefaultAzureCredentialOptions 
                    { 
                        ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") 
                    })))
                .Select(KeyFilter.Any, LabelFilter.Null)
                .Select(KeyFilter.Any, builderContext.HostingEnvironment.EnvironmentName)
                .UseFeatureFlags(ffo => ffo.SetRefreshInterval(TimeSpan.FromSeconds(5)));
            });
        }
        else
        {
            configurationBuilder.AddUserSecrets<Program>();
            
            // Configure AppSettings section for local development from individual settings
            if (builderContext.HostingEnvironment.IsEnvironment("Local") || builderContext.HostingEnvironment.IsDevelopment())
            {
                var localSettings = new Dictionary<string, string?>
                {
                    {"AppSettings:CosmosConnectionString", builderContext.Configuration["CosmosConnectionString"]},
                    {"AppSettings:StorageAccount", builderContext.Configuration["LocalBlobStorageConnectionString"]},
                };

                configurationBuilder.AddInMemoryCollection(localSettings);
            }
        }
    })
    .ConfigureServices((context, services) =>
        {
            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();
            services.ConfigureBlobStorage(context.Configuration, context.HostingEnvironment);
            services.ConfigureCosmos(context.Configuration, context.HostingEnvironment);
            services.AddSingleton<IEnvironmentProvider, LazyEnvironmentProvider>();
            services.AddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddScoped<IMainBlobStorageRepository, MainBlobStorageRepository>();
            services.AddScoped<ICosmosClientWrapper, CosmosClientWrapper>();
            services.AddScoped<ILearningContentService, LearningContentService>();
        }
    )
    .Build();

host.Run();