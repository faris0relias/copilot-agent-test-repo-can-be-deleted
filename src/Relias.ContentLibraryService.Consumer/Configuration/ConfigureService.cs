using Azure.Identity;
using MassTransit;
using MassTransit.Middleware;
using MassTransit.Monitoring;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Relias.ContentLibraryService.Common.Logging;
using Relias.ContentLibraryService.Common.Repositories;
using Relias.ContentLibraryService.Common.Services.Policy;
using Relias.ContentLibraryService.Consumer.Consumers;
using Relias.ContentLibraryService.Consumer.Models;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Infra.Repositories;
using Relias.HealthChecks;
using Serilog;
using Serilog.Enrichers.Sensitive;

namespace Relias.ContentLibraryService.Consumer.Configuration
{
    public static class ConfigureServices
    {
        private static readonly TimeSpan AppConfigCacheExpiration = TimeSpan.FromSeconds(60);
        private const string PolicyManagerMessageConsumer = "message-consumer";
        private const string LocalDevelopmentEnvironmentName = "LocalDevelopment";


        /// <summary>
        /// Configure providers and services for the application to compose
        /// </summary>
        /// <param name="webApplicationBuilder"><see cref="WebApplicationBuilder"/></param>
        /// <returns><see cref="WebApplicationBuilder"/></returns>
        public static WebApplicationBuilder Configure(this WebApplicationBuilder webApplicationBuilder)
        {
            webApplicationBuilder.ConfigureLogging();
            webApplicationBuilder.ConfigureAppConfiguration();
            webApplicationBuilder.Services.AddApplicationInsightsTelemetry(webApplicationBuilder.Configuration);
            webApplicationBuilder.ConfigureHttpClients();
            webApplicationBuilder.ConfigureApplicationServices();
            

            return webApplicationBuilder;
            
        }

        /// <summary>
        /// Configure Logging
        /// </summary>
        /// <param name="webApplicationBuilder"></param>
        /// <returns><see cref="WebApplicationBuilder"/></returns>
        private static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder webApplicationBuilder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(webApplicationBuilder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.With(new CorrelationPropertyEnricher())
                .Enrich.WithSensitiveDataMasking(options => { options.Mode = MaskingMode.Globally; })
                .CreateBootstrapLogger();

            webApplicationBuilder.Host.UseSerilog((context, provider, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(webApplicationBuilder.Configuration)
                    .WriteTo.ApplicationInsights(provider.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces)
                    .Enrich.FromLogContext()
                    .Enrich.With(new CorrelationPropertyEnricher())
                    .Enrich.WithSensitiveDataMasking(options => { options.Mode = MaskingMode.Globally; });
            });

            return webApplicationBuilder;
        }


        /// <summary>
        /// Setup application configuration
        /// </summary>
        /// <param name="webApplicationBuilder"></param>
        /// <returns></returns>
        private static WebApplicationBuilder ConfigureAppConfiguration(this WebApplicationBuilder webApplicationBuilder)
        {
            if (webApplicationBuilder.Environment.EnvironmentName == LocalDevelopmentEnvironmentName)
            {
                var configurationBuilder = webApplicationBuilder.Configuration as IConfigurationBuilder;
                configurationBuilder.AddUserSecrets<Program>();
                webApplicationBuilder.ConfigureAzureAppConfiguration();
                var environmentVariablesConfigurationSource = configurationBuilder.Sources.First(
                    configurationSource => configurationSource.GetType() == typeof(EnvironmentVariablesConfigurationSource)
                );
                configurationBuilder.Sources.Remove(environmentVariablesConfigurationSource);
                var jsonConfigurationSources = configurationBuilder.Sources.Where(
                    configurationSource => configurationSource.GetType() == typeof(JsonConfigurationSource)
                ).Cast<JsonConfigurationSource>();
                var userSecretsConfigurationSource = jsonConfigurationSources.First(configurationSource => configurationSource.Path == "secrets.json");
                configurationBuilder.Sources.Remove(userSecretsConfigurationSource);
                configurationBuilder.AddEnvironmentVariables();
                configurationBuilder.AddUserSecrets<Program>(true, true);
            }
            else
            {
                webApplicationBuilder.ConfigureAzureAppConfiguration();
            }

            return webApplicationBuilder;
        }

        /// <summary>
        /// Configure Azure App Configuration
        /// </summary>
        /// <param name="webApplicationBuilder"><see cref="WebApplicationBuilder"/></param>
        /// <returns><see cref="WebApplicationBuilder"/></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        private static WebApplicationBuilder ConfigureAzureAppConfiguration(this WebApplicationBuilder webApplicationBuilder)
        {
            var isDev = Convert.ToBoolean(webApplicationBuilder.Configuration["IS_LOCAL"] ?? "false");
            bool isIntegration = webApplicationBuilder.Environment.IsEnvironment("Integration");
           
            try
            {
                // if not dev read from appconfig, else read from app settings
                if(!isDev && !isIntegration)
                {
                    // non local uses DefaultAzureCredential
                    var appConfigEndpoint = webApplicationBuilder.Configuration["APPCONFIG_ENDPOINT"] ?? throw new KeyNotFoundException("APPCONFIG_ENDPOINT");
                    webApplicationBuilder.Configuration.AddAzureAppConfiguration(options =>
                    {
                        options.Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential())
                            .ConfigureKeyVault(kv =>
                            {
                                // Non-dev KV configuration (DefaultAzureCredential)
                                kv.SetCredential(new DefaultAzureCredential());
                            })
                            // Read values with no label first (if any),
                            // then override them with an environment-specific label (if any)
                            .Select(KeyFilter.Any, LabelFilter.Null)
                            .Select(KeyFilter.Any, webApplicationBuilder.Environment.EnvironmentName)
                            // Configure Refresh
                            .ConfigureRefresh(refresh =>
                            {
                                // Register sentinel key and configure cache expiration time (default is 30 seconds)
                                refresh.Register("CommonSettings:Sentinel", refreshAll: true).SetCacheExpiration(AppConfigCacheExpiration);
                            })
                            // Use feature flags with adjusted expiration time (default is 30 seconds)
                            .UseFeatureFlags(options => { options.CacheExpirationInterval = TimeSpan.FromSeconds(5); });
                    });
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                throw;
            }

#pragma warning disable S125 // Sections of code should not be commented out
            // If you would like the environment variables to override azure app config values,
            // (last in the chain wins), uncomment the line below.
            //builder.Configuration.AddEnvironmentVariables();
#pragma warning restore S125 // Sections of code should not be commented out

            webApplicationBuilder.Services.AddAzureAppConfiguration();
            webApplicationBuilder.Services.AddFeatureManagement();

            return webApplicationBuilder;
        }

        /// <summary>
        /// Configure HTTP Clients
        /// </summary>
        /// <param name="webApplicationBuilder"><see cref="WebApplicationBuilder"/></param>
        /// <returns><see cref="WebApplicationBuilder"/></returns>
        private static WebApplicationBuilder ConfigureHttpClients(
            this WebApplicationBuilder webApplicationBuilder)
        {

            return webApplicationBuilder;
        }

        /// <summary>
        /// Configure Application Services
        /// </summary>
        /// <param name="webApplicationBuilder"><see cref="WebApplicationBuilder"/></param>
        /// <returns><see cref="WebApplicationBuilder"/></returns>
        private static WebApplicationBuilder ConfigureApplicationServices(
            this WebApplicationBuilder webApplicationBuilder)
        {

            var baseSettings = webApplicationBuilder.Configuration.GetRequiredSection(nameof(BaseSettings)).Get<BaseSettings>();

            webApplicationBuilder.Services.Configure<BaseSettings>(webApplicationBuilder.Configuration.GetSection(nameof(BaseSettings)));

            webApplicationBuilder.Services.AddDbContext<ApplicationDbContext>((opt) =>
            {
                opt.UseSqlServer(baseSettings.SqlServerConnectionString);
            });

            webApplicationBuilder.Services.AddScoped<ApplicationDbContext, ApplicationDbContext>();
            webApplicationBuilder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
            webApplicationBuilder.Services.AddScoped<IPolicyService, PolicyService>();
            webApplicationBuilder.Services.AddOptions<BaseSettings>(nameof(BaseSettings));

            webApplicationBuilder.Services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.AddConsumer<PolicyPublishedConsumer>();
                busConfigurator.AddConsumer<PolicyArchivedConsumer>();
                busConfigurator.AddConsumer<PolicyUpdatedConsumer>();
                busConfigurator.AddConsumer<AssignContentConsumer>();
                busConfigurator.AddConsumer<PolicyAttestationCompletedConsumer>();

                busConfigurator.AddConfigureEndpointsCallback((ctx, name, config) => {
                    config.UseDelayedRedelivery(r => r.Interval(5, TimeSpan.FromMinutes(5)));
                    config.UseMessageRetry(r => r.Immediate(5));
                    config.ConfigureDeadLetterQueueDeadLetterTransport();
                    config.ConfigureDeadLetterQueueErrorTransport();
                }); 

                busConfigurator.UsingAzureServiceBus((ctx, cfg) =>
                {
                    cfg.Host(baseSettings.ServiceBusConnectionString);
                   
                    cfg.SubscriptionEndpoint(baseSettings.PolicyPublishedEventSubscription, baseSettings.PolicyPublishedEventTopicName, e =>
                    {
                        e.ConfigureConsumer<PolicyPublishedConsumer>(ctx);
                        e.PublishFaults = false;
                    });

                    cfg.SubscriptionEndpoint(baseSettings.PolicySyncEventSubscription, baseSettings.PolicySyncEventTopicName, e =>
                    {
                        e.ConfigureConsumer<PolicyPublishedConsumer>(ctx);
                        e.PublishFaults = false;
                    });

                    cfg.SubscriptionEndpoint(baseSettings.PolicyUpdatedEventSubscription, baseSettings.PolicyUpdatedEventTopicName, e =>
                    {                        
                        e.ConfigureConsumer<PolicyUpdatedConsumer>(ctx);
                        e.PublishFaults = false;
                    });

                    cfg.SubscriptionEndpoint(baseSettings.PolicyArchivedEventSubscription, baseSettings.PolicyArchivedEventTopicName, e =>
                    {
                        e.ConfigureConsumer<PolicyArchivedConsumer>(ctx);
                        e.PublishFaults = false;
                    });

                    cfg.SubscriptionEndpoint(baseSettings.AssignContentEventSubscription, baseSettings.AssignContentEventTopicName, e =>
                    {
                        e.ConfigureConsumer<AssignContentConsumer>(ctx);
                        e.PublishFaults = false;
                    });

                    cfg.SubscriptionEndpoint(baseSettings.PolicyAttestedEventSubscription, baseSettings.PolicyAttestedEventTopicName, e =>
                    {
                        e.ConfigureConsumer<PolicyAttestationCompletedConsumer>(ctx);
                        e.PublishFaults = false;
                    });
                });
            });

            // remove mass transit implicitly added health check(s) so we can explicitly add one with the 'readiness' tag
            webApplicationBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<HealthCheckServiceOptions>, RemoveMasstransitHealthChecks>());
            webApplicationBuilder.Services.AddPolicyMessageConsumerHealthChecks();

            return webApplicationBuilder;
        }

        public static WebApplication ConfigureHealthChecks(this WebApplication webApplication)
        {
            webApplication.MapHealthCheckDetailsEndpoint();
            webApplication.MapLivenessProbeEndpoint();
            webApplication.MapReadinessProbeEndpoint();
            webApplication.MapLoadBalancerProbeEndpoint();

            return webApplication;
        }

        /// <summary>
        /// Adds health checks
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPolicyMessageConsumerHealthChecks(this IServiceCollection services)
        {
            services
                 .AddHealthChecks()                 
                 .AddCheck<RedactedBusHealthCheck>(HealthCheckConstants.MasstransitBus, HealthStatus.Unhealthy,
                     tags: new[] { HealthCheckTags.ReadinessProbe });

            return services;
        }
    }
}
