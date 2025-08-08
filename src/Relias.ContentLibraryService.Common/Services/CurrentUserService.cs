using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Relias.ContentLibraryService.Common.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;
        public string OrganizationId => _httpContextAccessor.HttpContext?.User?.FindFirstValue("organization_id")!;
    }
}