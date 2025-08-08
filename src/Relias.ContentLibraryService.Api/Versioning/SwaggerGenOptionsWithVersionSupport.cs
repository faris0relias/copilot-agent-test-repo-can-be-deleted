using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Relias.ContentLibraryService.Api.Versioning;

/// <summary>
/// Configures SwaggerGen of the OpenAPI specification document with multi-version API support.
/// </summary>
public class SwaggerGenOptionsWithVersionSupport(
    IApiVersionDescriptionProvider apiVersionProvider,
    IHostEnvironment hostEnvironment)
    : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _apiVersionProvider = apiVersionProvider ?? throw new ArgumentNullException(nameof(apiVersionProvider));
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));

    public void Configure(SwaggerGenOptions options)
    {
//         var authSchemeDescription = !_hostEnvironment.IsDevelopment()
//             ? string.Empty
//             : """
//
//               Enter 'Bearer &lt;token&gt;' into the input below
//                
//               <b>Example:</b>
//                
//                   Bearer 1234567890abcdef
//
//             """;
//
//         var authScheme = new OpenApiSecurityScheme
//         {
//             Description = authSchemeDescription,
//             In = ParameterLocation.Header,
//             Name = "Authorization",
//             Scheme = "Bearer",
//             Type = SecuritySchemeType.ApiKey,
//         };

        // options.AddSecurityDefinition("Bearer", authScheme);
        //
        // options.AddSecurityRequirement(new OpenApiSecurityRequirement
        // {
        //     {
        //         new OpenApiSecurityScheme
        //         {
        //             In = ParameterLocation.Header,
        //             Name = "Bearer",
        //             Reference = new OpenApiReference
        //             {
        //                 Id = "Bearer",
        //                 Type = ReferenceType.SecurityScheme,
        //             },
        //             Scheme = "oauth2",
        //         },
        //         Array.Empty<string>()
        //     },
        // });

        options.DocumentFilter<RemoveVersionPrefixFromIndividualPathsDocumentFilter>();

        foreach (var description in _apiVersionProvider.ApiVersionDescriptions)
        {
            var apiDisplayVersion = description.ApiVersion.ToString();
            if (!apiDisplayVersion.StartsWith('v'))
            {
                apiDisplayVersion = $"v{apiDisplayVersion}";
            }

            // options.SwaggerDoc(description.GroupName, new OpenApiInfo()
            // {
            //     Title = $"Content Library Service  - {apiDisplayVersion}",
            //     Version = description.IsDeprecated ? $"{description.ApiVersion} (Deprecated)" : description.ApiVersion.ToString(),
            // });

            options.CustomSchemaIds(type => type.IsGenericType ? SanitizeGenericTypeName(type) : type.FullName);
        }
    }

    private static string SanitizeGenericTypeName(Type t)
    {
        return $"{t.Namespace}.{t.Name.Replace("`", "-", StringComparison.Ordinal)}.{t.GetGenericArguments()[0].ToString().Replace("+", string.Empty, StringComparison.Ordinal)}";
    }
}

