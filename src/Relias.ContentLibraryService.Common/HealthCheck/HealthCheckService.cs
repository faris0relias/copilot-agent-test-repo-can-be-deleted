using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Relias.ContentLibraryService.Common.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly IEnumerable<IDependencyHealthCheck> _dependencies;
        private readonly IEnvironmentProvider _environmentProvider;
        private readonly object _lock = new();
        
        public HealthCheckService(
        IEnvironmentProvider environmentProvider,
        IEnumerable<IDependencyHealthCheck> dependencies)
        {
            _environmentProvider = environmentProvider ?? throw new ArgumentNullException(nameof(environmentProvider));
            _dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
        }

        public async Task<Tuple<bool, string>> GetHealthStatusAsync()
        {
            var sb = new StringBuilder();

            // Display when this started
            sb.AppendLine($"{DateTimeOffset.UtcNow} (UTC)");
            sb.AppendLine($"Host name:  {Environment.MachineName}");
            sb.AppendLine($"Environment: {_environmentProvider.EnvironmentName}");
            sb.AppendLine($"Deployed on: {File.GetLastWriteTimeUtc(Assembly.GetEntryAssembly()!.Location)} (UTC)");
            sb.AppendLine();

            var stopwatchTotal = new Stopwatch();
            stopwatchTotal.Start();

            var healthCheckTasks = _dependencies.Select(d => ProcessCheckAsync(d.ServiceName, d.CheckServiceHealth(), sb));
            var results = await Task.WhenAll(healthCheckTasks);

            stopwatchTotal.Stop();
            sb.AppendLine();

            // Output the total time
            var totalTime = stopwatchTotal.ElapsedMilliseconds.ToString("n0");

            var success = results.All(x => x);
            if (success)
            {
                sb.AppendLine($"[ALL OK]\t\t{totalTime} ms");
            }
            else
            {
                sb.AppendLine($"[FAIL]\t\t{totalTime} ms");
            }

            return new Tuple<bool, string>(success, sb.ToString());
        }

        private async Task<bool> ProcessCheckAsync(string name, Task task, StringBuilder sb)
        {
            // Build and start a stopwatch to monitor time to process check
            var sw = new Stopwatch();
            sw.Start();
            var completedOk = true;

            try
            {
                await task;
            }
            catch
            {
                // Assume any exception means something bad, consider failed
                completedOk = false;
            }
            finally
            {
                sw.Stop();
            }

            // Time to process
            var span = sw.ElapsedMilliseconds.ToString("n0");

            // Build status line
            var text = string.Concat(name, "\t", completedOk ? "OK\t" : "FAIL\t", span, " ms");

            lock (_lock)
            {
                // Lock for thread safety
                sb.AppendLine(text);
            }

            return completedOk;
        }
    }
}