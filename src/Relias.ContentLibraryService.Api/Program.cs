using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Relias.ContentLibraryService.Api.Middleware;
using Relias.ContentLibraryService.Api.Startup;
using Relias.ContentLibraryService.Common.Logging;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System.Diagnostics.CodeAnalysis;


[assembly: InternalsVisibleTo("Relias.ContentLibraryService.UnitTests")]
var builder = WebApplication.CreateBuilder(args);

const string PrimaryAppConfigurationEndpointKey = "APPCONFIG_ENDPOINT_PRIMARY";
const string SecondaryAppConfigurationEndpointKey = "APPCONFIG_ENDPOINT_SECONDARY";

if (!builder.Environment.IsDevelopment()
    && !builder.Environment.IsEnvironment("Integration"))
{
    var appConfigEndpoints = new List<Uri> {
        new(builder.Configuration[PrimaryAppConfigurationEndpointKey] ?? throw new KeyNotFoundException(PrimaryAppConfigurationEndpointKey)),
        new(builder.Configuration[SecondaryAppConfigurationEndpointKey] ?? throw new KeyNotFoundException(SecondaryAppConfigurationEndpointKey))
    };

    builder.Configuration.AddAzureAppConfiguration(options =>

        options.Connect(appConfigEndpoints, new ManagedIdentityCredential())
            .ConfigureKeyVault(static keyVaultOptions => keyVaultOptions.SetCredential(new ManagedIdentityCredential()))
            .Select(KeyFilter.Any, LabelFilter.Null)
            .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
            .UseFeatureFlags(options => options.CacheExpirationInterval = TimeSpan.FromSeconds(5))
    );
}

builder.Services.AddAzureAppConfiguration();

builder.AddWebAppBuilderConfigurations();

var loggerConfig = new LoggerConfiguration()
  .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
  .Enrich.FromLogContext()
  .Enrich.With(new CorrelationPropertyEnricher())
  .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3}] {Username} {Message:lj}{NewLine}{Exception}")
  .WriteTo.ApplicationInsights(builder.Configuration["ApplicationInsights:ConnectionString"], new TraceTelemetryConverter())
  .CreateLogger();

builder.Host.UseSerilog(loggerConfig);

var app = builder.Build();

//await app.Services.SeedDataAsync();

// CORS Policy
app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});

// Health Check Mapping
app.MapHealthChecks("/HealthCheck", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = HealthCheckResponseWriter.WritePlainTextHealthCheckResponse
});

// Development Environment Configuration
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("dev1") || app.Environment.IsEnvironment("dev2"))
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var groupName in provider.ApiVersionDescriptions.Select(x => x.GroupName))
        {
            options.SwaggerEndpoint($"/swagger/{groupName}/swagger.json",
                $"Content Library Service API {groupName.ToUpperInvariant()}");
        }
    });
}

// HTTPS Redirection, Routing, and Authorization
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

[ExcludeFromCodeCoverage(Justification = "Program entry point")]
public partial class Program
{
    protected Program() { }
}