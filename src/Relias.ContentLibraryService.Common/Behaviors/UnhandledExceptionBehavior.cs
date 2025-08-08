using MediatR;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Relias.ContentLibraryService.Common.Behaviors
{
    
    public class UnhandledExceptionBehavior<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;
                var appName = Assembly.GetExecutingAssembly().GetName().Name;

                logger.LogError(ex, "{@appName} Request: Unhandled Exception for Request {Name} {@Request}", appName, requestName, request);

                throw;
            }
        }
    }
}