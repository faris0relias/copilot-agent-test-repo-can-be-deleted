namespace Relias.ContentLibraryService.Common.Resilience;

public interface IResilienceExecutor
{
    Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken);
}
