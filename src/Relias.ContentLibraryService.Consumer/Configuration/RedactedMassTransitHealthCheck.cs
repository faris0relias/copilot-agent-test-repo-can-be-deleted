using MassTransit;
using MassTransit.Transports;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.RegularExpressions;

namespace Relias.ContentLibraryService.Consumer.Configuration
{
    public class RedactedBusHealthCheck : IHealthCheck
    {
        readonly IBusInstance _busInstance;

        public RedactedBusHealthCheck(IBusInstance busInstance)
        {
            _busInstance = busInstance;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            var result = _busInstance.BusControl.CheckHealth();

            string pattern = "^(.*?)\\.net";
            Regex regex = new Regex(pattern, RegexOptions.Compiled);

            var data = new Dictionary<string, object>
            {
                ["Endpoints"] = new EndpointDictionary(result.Endpoints.ToDictionary(x => regex.Replace(x.Key, "redacted-sb-host"),
                    x => new Endpoint(Enum.GetName(typeof(BusHealthStatus), x.Value.Status), x.Value.Description)
                ))
            };

            var minimalHealthcheckLevel = context.Registration.FailureStatus switch
            {
                HealthStatus.Healthy => BusHealthStatus.Healthy,
                HealthStatus.Degraded => BusHealthStatus.Degraded,
                _ => BusHealthStatus.Unhealthy
            };

            var usedHealthcheckResult = result.Status < minimalHealthcheckLevel ? minimalHealthcheckLevel : result.Status;

            return Task.FromResult(usedHealthcheckResult switch
            {
                BusHealthStatus.Healthy => HealthCheckResult.Healthy(result.Description, data),
                BusHealthStatus.Degraded => HealthCheckResult.Degraded(result.Description, result.Exception, data),
                _ => HealthCheckResult.Unhealthy(result.Description, result.Exception, data)
            });
        }


        class EndpointDictionary :
            Dictionary<string, Endpoint>
        {
            public EndpointDictionary(IDictionary<string, Endpoint> dictionary)
                : base(dictionary, StringComparer.OrdinalIgnoreCase)
            {
            }

            public override string ToString()
            {
                return string.Join(", ", this.Select(x => $"{x.Key}: {x.Value}"));
            }
        }


        class Endpoint
        {
            public Endpoint(string status, string description)
            {
                Status = status;
                Description = description;
            }

            public string Status { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return $"{Status} - {Description}";
            }
        }
    }
}
