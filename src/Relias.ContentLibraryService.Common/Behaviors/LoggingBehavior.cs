using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.Common.Services;

namespace Relias.ContentLibraryService.Common.Behaviors
{
    public class LoggingBehavior<TRequest>(ILogger<TRequest> logger, ICurrentUserService currentUserService) : IRequestPreProcessor<TRequest>
        where TRequest : notnull
    {
        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = currentUserService.UserId ?? string.Empty;

            return Task.Run(
                () => logger.LogInformation(
                    "Relias Request: {Name} {@UserId} {@Request}",
                    requestName,
                    userId,
                    request),
                cancellationToken);
        }
    }
}