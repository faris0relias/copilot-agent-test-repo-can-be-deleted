using System.Reflection;

namespace Relias.ContentLibraryService.Api.Startup
{
    public static class ConfigurationExtensions
    {
        public static WebApplicationBuilder AddWebAppBuilderConfigurations(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            // Discover all classes implementing INeedCustomConfiguration
            var configurationTypes = GetCustomTypes<CustomWebAppBuilderConfigurationAttribute>();
            
            return configurationTypes == null
                ? builder
                : configurationTypes.Select(Activator.CreateInstance).Aggregate(
                    builder,
                    (current, instance) => ((INeedWebAppBuilderConfiguration)instance!).Configure(current));
        }

        public static WebApplication AddWebApplicationConfigurations(this WebApplication builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            // Discover all classes implementing INeedCustomConfiguration
            var configurationTypes = GetCustomTypes<CustomWebAppConfigurationAttribute>();

            return configurationTypes == null
                ? builder
                : configurationTypes.Select(Activator.CreateInstance).Aggregate(
                    builder,
                    (current, instance) => ((INeedWebAppConfiguration)instance!).Configure(current));
        }

        private static IEnumerable<Type>? GetCustomTypes<T>()
            where T : Attribute
            => Assembly.GetEntryAssembly()
                ?.GetTypes()
                .Where(t => t.GetCustomAttribute<T>() is not null);
    }
}