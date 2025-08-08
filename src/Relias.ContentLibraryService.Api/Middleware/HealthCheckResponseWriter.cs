using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;

namespace Relias.ContentLibraryService.Api.Middleware;

public static class HealthCheckResponseWriter
{
    public static Task WritePlainTextHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "text/plain";
        var formattedReport = FormatReport(report);
        return context.Response.WriteAsync(formattedReport);
    }

    public static string FormatReport(HealthReport report)
    {
        var sb = new StringBuilder();

        sb.Append(Environment.MachineName).Append(' ').Append(DateTime.UtcNow).AppendLine();
        sb.Append("Environment: ").AppendLine(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        sb.AppendLine();

        foreach (var (name, value) in report.Entries)
        {
            const int nameStatusMaxLength = 15;
            const int nameAndGapMaxLength = 27;

            var status = value.Status == HealthStatus.Unhealthy ? "FAIL" : "OK";
            var duration = value.Duration.TotalMilliseconds.ToString("n0");
            var nameStatus = $"{name}{AddSpaces(nameStatusMaxLength, name.Length)} {status}";
            sb.Append(nameStatus).Append(' ').Append(AddSpaces(nameAndGapMaxLength, nameStatus.Length)).Append(duration).AppendLine(" ms");
        }

        if (report.Status != HealthStatus.Unhealthy)
        {
            sb.AppendLine("ALL OK");
        }

        sb.AppendLine();
        sb.Append(
            "Build: ").Append(File.GetLastWriteTime(typeof(HealthCheckResponseWriter).Assembly.Location).ToUniversalTime()).AppendLine();

        return sb.ToString();
    }

    private static string AddSpaces(int maxSpaces, int textLength)
    {
        var spacesNeeded = maxSpaces - textLength;
        return spacesNeeded > 0 ? new string(' ', spacesNeeded) : string.Empty;
    }
}
