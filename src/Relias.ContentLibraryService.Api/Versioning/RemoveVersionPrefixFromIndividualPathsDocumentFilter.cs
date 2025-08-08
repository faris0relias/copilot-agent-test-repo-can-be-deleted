using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Relias.ContentLibraryService.Api.Versioning;

/// <summary>
/// When generating open api specification documents for versioned apis we need to correct the swashbuckle default behavior of including
/// the entire path in the individual operation path. The api and version prefix portions of the route need to instead be made
/// part of the servers/base url definition.
/// </summary>
public class RemoveVersionPrefixFromIndividualPathsDocumentFilter : IDocumentFilter
{
    private const string Prefix = "api";

    /// <inheritdoc />
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        try
        {
            _ = swaggerDoc ?? throw new ArgumentNullException(nameof(swaggerDoc));

            var paths = new OpenApiPaths();

            // If the minor version is zero (e.g. 1.0) the route prefix gets shortened to v1
            // but any other minor version value gets left in the route such as v1.1
            // We'll need to normalize for both scenarios.
            var apiVersion = swaggerDoc.Info.Version;
            if (apiVersion.EndsWith(".0", StringComparison.Ordinal))
            {
                apiVersion = apiVersion[..apiVersion.LastIndexOf(".0", StringComparison.Ordinal)];
            }

            // var apiAndVersionPrefix = $"/{Prefix}";
            //
            // foreach (var keyValuePair in swaggerDoc.Paths)
            // {
            //     var normalizedPath = keyValuePair.Key.Replace(apiAndVersionPrefix, string.Empty, StringComparison.Ordinal);
            //     if (!paths.ContainsKey(normalizedPath))
            //     {
            //         paths.Add(normalizedPath, keyValuePair.Value);
            //     }
            // }

            // swaggerDoc.Paths = paths;
            //
            // var openApiServer = new OpenApiServer()
            // {
            //     Url = apiAndVersionPrefix,
            // };

            //swaggerDoc.Servers.Add(openApiServer);
        }
        catch (Exception filterException)
        {
            throw new SwaggerGeneratorException("Failed to apply the RemoveVersionPrefixFromIndividualPathsDocumentFilter", filterException);
        }
    }
}
