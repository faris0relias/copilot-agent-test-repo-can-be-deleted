using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Relias.ContentLibraryService.Common.Services.Extension
{
    [ExcludeFromCodeCoverage(Justification = "Health Check")]

    public static class HealthCheckConfigurationExtenstions
    {
        // public static IServiceCollection AddServiceBusHealthCheck(this IServiceCollection services, string connectionString, string queueName)
        // {
        //     services.AddSingleton<IDependencyHealthCheck>(sp => new ServiceBusQueueHealthCheck(connectionString, queueName));
        //     return services;
        // }

        public static IServiceCollection AddSqlServerHealthCheck(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IDependencyHealthCheck>(sp => new SqlServerHealthCheck(connectionString));
            return services;
        }
        
    }
}