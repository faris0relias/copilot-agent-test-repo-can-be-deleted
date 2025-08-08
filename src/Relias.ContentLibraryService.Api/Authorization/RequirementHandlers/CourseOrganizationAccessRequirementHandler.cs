using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Relias.ContentLibraryService.Api.Utilities;
using Relias.ContentLibraryService.App.Interfaces;
using System.Security.Claims;

namespace Relias.ContentLibraryService.Api.Authorization.RequirementHandlers;

public class CourseOrganizationAccessRequirement : IAuthorizationRequirement
{
    public CourseOrganizationAccessRequirement() { }
}

public class CourseOrganizationAccessRequirementHandler : AuthorizationHandler<CourseOrganizationAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICourseService _courseService;
    private readonly ILogger<CourseOrganizationAccessRequirementHandler> _logger;

    public CourseOrganizationAccessRequirementHandler(
        IHttpContextAccessor httpContextAccessor,
        ICourseService courseService,
        ILogger<CourseOrganizationAccessRequirementHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _courseService = courseService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CourseOrganizationAccessRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            context.Fail();
            return;
        }

        if (_httpContextAccessor.HttpContext == null)
        {
            context.Fail();
            return;
        }

        var request = _httpContextAccessor.HttpContext.Request;
        var routeValues = _httpContextAccessor.HttpContext.GetRouteData()?.Values;

        Guid courseId = Guid.Empty;

        var hasCourseId = routeValues?.TryGetValue("courseId", out var courseIdValue) == true &&
                          Guid.TryParse(courseIdValue?.ToString(), out courseId);

        if (!hasCourseId)
        {
            _logger.LogWarning("courseId not found or invalid in route.");
            context.Succeed(requirement);
            return;
        }

        var course = await _courseService.GetByIdAsync(courseId, request.HttpContext.RequestAborted);
        if (course == null)
        {
            _logger.LogWarning("Course with ID {CourseId} not found.", courseId);
            context.Succeed(requirement);
            return;
        }
        
        int courseOrgId = course.OrganizationId;
        var userOrgId = context.User.OrganizationId();
        var subOrgIds = context.User.OrganizationIds();

        bool hasAccess = userOrgId == 1 || userOrgId == courseOrgId || subOrgIds.Contains(courseOrgId);

        if (hasAccess)
        {
            context.Succeed(requirement);
            return;
        }

        _logger.LogWarning("User not authorized to access course {CourseId} with org {OrganizationId}", courseId, courseOrgId);
        context.Fail();
    }
}
