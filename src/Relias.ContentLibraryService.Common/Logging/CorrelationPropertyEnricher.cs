using Serilog.Core;
using Serilog.Events;

namespace Relias.ContentLibraryService.Common.Logging;

/// <summary>
/// Updates the Serilog log with the correlation Id taken from System.Diagnostics.Activity.Current.Id value.
/// Azure App insights use this property for distributed tracking, so this links Serilig and App Insights logs.
/// </summary>
public class CorrelationPropertyEnricher : ILogEventEnricher
{
    const string CORRELATION_PROPERTY_NAME = "CorrelationId";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        string? currentActivityId = System.Diagnostics.Activity.Current?.Id;

        if (!string.IsNullOrWhiteSpace(currentActivityId))
        {
            LogEventProperty correlationProperty = propertyFactory.CreateProperty(CORRELATION_PROPERTY_NAME, currentActivityId);
            logEvent.AddOrUpdateProperty(correlationProperty);
        }
    }
}
