using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Relias.ContentLibraryService.Common.Versioning;

/// <summary>
/// Configures Swagger with multi-version API support
/// </summary>
public class SwaggerGenOptionsWithVersionSupport : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public SwaggerGenOptionsWithVersionSupport(IApiVersionDescriptionProvider provider) => _provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
        foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = Assembly.GetEntryAssembly()?.GetName().Name ?? "API",
                Version = description.IsDeprecated ? $"{description.ApiVersion} (Deprecated)" : description.ApiVersion.ToString()
            });
        }
    }
}