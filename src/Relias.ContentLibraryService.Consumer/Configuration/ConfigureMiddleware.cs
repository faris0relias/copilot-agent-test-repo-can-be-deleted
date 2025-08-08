using Serilog;

namespace Relias.ContentLibraryService.Consumer.Configuration
{
    public static class ConfigureMiddleware
    {
        public static WebApplication Configure(this WebApplication webApplication)
        {
            webApplication.UseSerilogRequestLogging();

            webApplication.ConfigureHealthChecks();

            return webApplication;
        }

 
    }
}