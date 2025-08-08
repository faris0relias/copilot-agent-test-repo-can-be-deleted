namespace Relias.ContentLibraryService.Api.Startup
{
    public interface INeedWebAppBuilderConfiguration
    {
        WebApplicationBuilder Configure(WebApplicationBuilder builder);
    }
}