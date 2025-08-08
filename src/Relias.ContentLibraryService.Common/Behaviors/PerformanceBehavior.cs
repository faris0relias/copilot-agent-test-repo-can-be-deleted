using MediatR;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.Common.Services;
using System.Diagnostics;

namespace Relias.ContentLibraryService.Common.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse>(
        ILogger<TRequest> logger,
        ICurrentUserService currentUserService) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly Stopwatch _timer = new();

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            if (elapsedMilliseconds <= 500)
            {
                return response;
            }

            var requestName = typeof(TRequest).Name;
            var userId = currentUserService.UserId ?? string.Empty;

            logger.LogWarning(
                "Relias Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}",
                requestName,
                elapsedMilliseconds,
                userId,
                request);

            return response;
        }
    }
}