using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.Common.Services;

namespace Relias.ContentLibraryService.Function.Function
{
    public class HealthCheckEndpoint
    {
        private readonly ILogger<HealthCheckEndpoint> _logger;
        private readonly IHealthCheckService _healthCheckService;
    
        public HealthCheckEndpoint(ILogger<HealthCheckEndpoint> logger, IHealthCheckService healthCheckService)
        {
            _logger = logger;
            _healthCheckService = healthCheckService;
        }
        
        [Function("HealthZ")]
        public HttpResponseData GetHealthZAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req
        ) =>
            req.CreateResponse(HttpStatusCode.OK);
    
        [Function("HealthCheck")]
        public async Task<HttpResponseData> GetHealthStatusAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
            HttpRequestData req
        )
        {
            var (isHealthy, healthStatus) = await _healthCheckService.GetHealthStatusAsync();
    
            var httpStatus = HttpStatusCode.OK;
            if (!isHealthy)
            {
                httpStatus = HttpStatusCode.ServiceUnavailable;
                _logger.LogError("Health check failed: {HealthStatus}", healthStatus);
            }
    
            var response = req.CreateResponse(httpStatus);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await response.WriteStringAsync(healthStatus);
            return response;
        }
    }
}