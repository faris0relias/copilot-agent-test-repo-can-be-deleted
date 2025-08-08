using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Relias.ContentLibraryService.Api.Logging;

public class CustomTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is ExceptionTelemetry exceptionTelemetry)
            {
                ((ISupportSampling)exceptionTelemetry).SamplingPercentage = 100;
            }
        }
    }
