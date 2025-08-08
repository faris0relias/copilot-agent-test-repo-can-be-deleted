using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using System.Diagnostics.CodeAnalysis;

namespace Relias.ContentLibraryService.Common.Resilience;

[ExcludeFromCodeCoverage]
public static class ResiliencePipelineExtensions
{
    public static void AddContentLibraryResilience(this IServiceCollection services)
    {
        services.AddResiliencePipeline(ResiliencePipelineNames.LearningContentCosmos, builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                MaxDelay = TimeSpan.FromSeconds(5),
                UseJitter = true,
                ShouldHandle = args =>
                {
                    if (args.Outcome.Exception is null)
                    {
                        return PredicateResult.False();
                    }

                    return args.Outcome.Exception switch
                    {
                        OperationCanceledException => PredicateResult.False(),
                        _ => PredicateResult.True()
                    };
                }
            });
        });

        services.AddScoped<IResilienceExecutor, ResilienceExecutor>();
    }
}
