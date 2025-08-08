using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Relias.ContentLibraryService.Consumer.Configuration
{
    public class RemoveMasstransitHealthChecks : IConfigureOptions<HealthCheckServiceOptions>
    {
        public void Configure(HealthCheckServiceOptions options)
        {
            var masstransitChecks = options.Registrations.Where(x => x.Tags.Contains(HealthCheckConstants.Masstransit)).ToList();

            foreach (var check in masstransitChecks)
            {
                options.Registrations.Remove(check);
            }
        }
    }
}
