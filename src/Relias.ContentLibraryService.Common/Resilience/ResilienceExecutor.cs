using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Relias.ContentLibraryService.Common.Resilience;

public class ResilienceExecutor(
    [FromKeyedServices(ResiliencePipelineNames.LearningContentCosmos)]
    ResiliencePipeline pipeline
) : IResilienceExecutor
{
    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        await pipeline.ExecuteAsync(ct => new ValueTask(action(ct)), cancellationToken);
    }
}
