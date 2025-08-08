using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Relias.ContentLibraryService.Api.Middleware;


namespace Relias.ContentLibraryService.Api.Startup
{
    public class WebAppConfiguration : INeedWebAppConfiguration
    {
        public WebApplication Configure(WebApplication app)
        {
            if (app.Environment.IsDevelopment()
                || app.Environment.IsEnvironment("dev1")
                || app.Environment.IsEnvironment("dev2"))
            {
                // Enable Swagger in non-prod environments only
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var groupName in provider.ApiVersionDescriptions.Select(x => x.GroupName))
                    {
                        options.SwaggerEndpoint($"/swagger/{groupName}/swagger.json",
                            $"User Service API {groupName.ToUpperInvariant()}");
                    }
                });
            }
            // app.UseEndpoints(endpoints =>
            // {
            //     endpoints.MapControllers();  // Maps your HealthCheckController
            //
            //     // Add a fallback route to test if the request is reaching the server
            //     endpoints.MapGet("/", async context =>
            //     {
            //         await context.Response.WriteAsync("The request reached the server, but no route matched.");
            //     });
            // });
            //
            // app.MapHealthChecks("/api/HealthCheck");
            // app.MapHealthChecks("/HealthCheck", new HealthCheckOptions
            // {
            //     ResponseWriter = HealthCheckResponseWriter.WritePlainTextHealthCheckResponse
            // });
            // app.MapGet("/", () => "Hello World");
            //
            // app.UseCors(builder => builder
            //     .AllowAnyOrigin()
            //     .AllowAnyMethod()
            //     .AllowAnyHeader());
            // app.UseHttpsRedirection();
            // app.UseRouting();
            // app.UseExceptionHandler(app.Environment.IsDevelopment() ? "/error-development" : "/error");
            // app.UseAuthentication();
            // app.UseAuthorization();
            // app.MapControllers();

            return app;
        }
    }
}