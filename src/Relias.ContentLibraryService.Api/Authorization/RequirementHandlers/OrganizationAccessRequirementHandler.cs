using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Text;
using Relias.ContentLibraryService.Api.Utilities;

namespace Relias.ContentLibraryService.Api.Authorization.RequirementHandlers;

public class OrganizationAccessRequirement : IAuthorizationRequirement
{
    public OrganizationAccessRequirement() { }
}

public class OrganizationAccessRequirementHandler : AuthorizationHandler<OrganizationAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<OrganizationAccessRequirementHandler> _logger;

    public OrganizationAccessRequirementHandler(IHttpContextAccessor httpContextAccessor, ILogger<OrganizationAccessRequirementHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganizationAccessRequirement requirement)
    {
        if(context.User?.Identity?.IsAuthenticated != true)
        {
            context.Fail();
            return;
        };

        var hasAccess = false;

        if (_httpContextAccessor.HttpContext == null)
        {
            context.Fail();
            return;
        }

        if (_httpContextAccessor.HttpContext.User.Identity is { IsAuthenticated: false })
        {
            context.Fail();
            return;
        }

        // Try getting the org id from the route
        var requestHasOrgId = int.TryParse(_httpContextAccessor.HttpContext.Request.RouteValues[ReliasRequestValues.OrganizationId]?.ToString(), out var requestedOrgId);

        if (!requestHasOrgId)
        {
            // Since the route doesn't have the orgId, try getting it from the params
            _httpContextAccessor.HttpContext.Request.Query.TryGetValue(ReliasRequestValues.OrganizationId, out var org);
            if (!string.IsNullOrEmpty(org))
            {
                requestHasOrgId = int.TryParse(org, out requestedOrgId);
            }
        }

        if (!requestHasOrgId)
        {
            // Since the params don't have the orgId, try getting it from the body
            requestedOrgId = await GetBodyOrgId(_httpContextAccessor.HttpContext.Request);
            requestHasOrgId = requestedOrgId > 0;
        }

        if (requestHasOrgId)
        {
            var orgId = context.User.OrganizationId();
            var subportalOrgIds = context.User.OrganizationIds();

            // User has access if they're a site1 user, if they're in the org they're targeting, or if the org they're targeting is among their org's sub orgs
            hasAccess = orgId == 1 || orgId == requestedOrgId || subportalOrgIds.Contains(requestedOrgId);
        }
        
        if (hasAccess)
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }

    private async Task<int> GetBodyOrgId(HttpRequest request)
    {
        var orgId = 0;

        // Attempt to get the body's org id. If trouble occurs, log the exception and let the handler fail the check
        try
        {
            var encoding = new UTF8Encoding();
            request.EnableBuffering();
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];

            await request.Body.ReadExactlyAsync(buffer, 0, buffer.Length);

            var body = encoding.GetString(buffer);
            var bodyObj = JsonSerializer.Deserialize<JsonElement>(body.ToLower());

            bodyObj.TryGetProperty(ReliasRequestValues.OrganizationId.ToLower(), out var orgIdElement);
            orgId = int.Parse(orgIdElement.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogInformation(
                ex,
                "Body could not be interpreted for request {requestName} in OrgAccess handler because of the following reason: {exception}",
                request.Path,
                ex);
        }
        finally
        {
            // No matter what happens, the position of the stream needs to be reset so the controller can read the body
            request.Body.Position = 0L;
        }

        return orgId;
    }
}

