using MediatR;
using Microsoft.AspNetCore.Authorization;
using Relias.ContentLibraryService.Common.Services;
using System.Reflection;

namespace Relias.ContentLibraryService.Common.Behaviors
{
    public class AuthorizationBehavior<TRequest, TResponse>(
        ICurrentUserService currentUserService) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

            if (!authorizeAttributes.Any())
            {
                return next();
            }

            // Must be authenticated user
            if (currentUserService.UserId == null)
            {
                throw new UnauthorizedAccessException();
            }

            // User is authorized / authorization not required
            return next();
        }
    }
}