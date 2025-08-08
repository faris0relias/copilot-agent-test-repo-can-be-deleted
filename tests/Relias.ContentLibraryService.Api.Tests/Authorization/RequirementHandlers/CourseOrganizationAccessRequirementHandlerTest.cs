using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Relias.ContentLibraryService.Api.Authorization;
using Relias.ContentLibraryService.Api.Authorization.RequirementHandlers;
using Relias.ContentLibraryService.Api.Utilities;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;
using System.Security.Claims;

namespace Relias.ContentLibraryService.Api.Tests.Authorization.RequirementHandlers;

public class CourseOrganizationAccessRequirementHandlerTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ICourseService> _courseServiceMock;
    private readonly Mock<ILogger<CourseOrganizationAccessRequirementHandler>> _loggerMock;
    private readonly CourseOrganizationAccessRequirementHandler _handler;
    private readonly CourseOrganizationAccessRequirement _requirement;
    private readonly DefaultHttpContext _httpContext;
    
    public CourseOrganizationAccessRequirementHandlerTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _courseServiceMock = new Mock<ICourseService>();
        _loggerMock = new Mock<ILogger<CourseOrganizationAccessRequirementHandler>>();
        
        _handler = new CourseOrganizationAccessRequirementHandler(
            _httpContextAccessorMock.Object,
            _courseServiceMock.Object,
            _loggerMock.Object);
        
        _requirement = new CourseOrganizationAccessRequirement();
        
        _httpContext = new DefaultHttpContext();
    }

    [Fact]
    public async Task UnauthenticatedUser_FailsAuthorization()
    {
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            new ClaimsPrincipal(),
            null);
        
        await _handler.HandleAsync(context);
        
        Assert.False(context.HasSucceeded);
    }
    
    [Fact]
    public async Task HandleRequirementAsync_WhenHttpContextIsNull_ShouldFailAuthorization()
    {
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "1"),
            new Claim(UserTokenKeys.OrganizationIds, "3"),
            new Claim(UserTokenKeys.OrganizationIds, "42") // Include course org ID
        }, "test"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
            
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            userClaims,
            null);
            
        await _handler.HandleAsync(context);
        
        Assert.False(context.HasSucceeded);
    }
    
    [Fact]
    public async Task MissingCourseIdInRoute_SucceedsAuthorization()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "1"),
            new Claim(UserTokenKeys.OrganizationIds, "3"),
            new Claim(UserTokenKeys.OrganizationIds, "42") // Include course org ID
        }, "test"));
        
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);
        
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(_httpContext);
        _httpContext.Features.Set<IRouteValuesFeature>(new RouteValuesFeature { RouteValues = new RouteValueDictionary() });
        
        await _handler.HandleAsync(context);
        
        Assert.True(context.HasSucceeded);
    }
    
    [Fact]
    public async Task InvalidCourseIdFormat_SucceedsAuthorization()
    {
        // Setup
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "1"),
            new Claim(UserTokenKeys.OrganizationIds, "3"),
            new Claim(UserTokenKeys.OrganizationIds, "42") // Include course org ID
        }, "test"));
        
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);
        
        var routeValues = new RouteValueDictionary { ["courseId"] = "not-a-guid" };
        
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(_httpContext);
        _httpContext.Features.Set<IRouteValuesFeature>(new RouteValuesFeature { RouteValues = routeValues });
        
        await _handler.HandleAsync(context);
        
        Assert.True(context.HasSucceeded);
    }
    
    [Fact]
    public async Task CourseNotFound_SucceedsAuthorization()
    {
        // Setup
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "1"),
            new Claim(UserTokenKeys.OrganizationIds, "3"),
            new Claim(UserTokenKeys.OrganizationIds, "42") // Include course org ID
        }, "test"));
        
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);
        
        var courseId = Guid.NewGuid();
        var routeValues = new RouteValueDictionary { ["courseId"] = courseId.ToString() };
        
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(_httpContext);
        _httpContext.Features.Set<IRouteValuesFeature>(new RouteValuesFeature { RouteValues = routeValues });
        
        _courseServiceMock.Setup(s => s.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CourseDto)null!);
        
        await _handler.HandleAsync(context);
        
        Assert.True(context.HasSucceeded);
    }
    
    [Fact]
    public async Task SuperAdminUser_SucceedsAuthorization()
    {
        var courseId = Guid.NewGuid();
        
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "1"),
            new Claim(UserTokenKeys.OrganizationIds, "3"),
            new Claim(UserTokenKeys.OrganizationIds, "42") 
        }, "test"));
        
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            userClaims,
            null);
        
        var routeValues = new RouteValueDictionary { ["courseId"] = courseId.ToString() };
        
        _httpContext.User = userClaims;
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(_httpContext);
        _httpContext.Features.Set<IRouteValuesFeature>(new RouteValuesFeature { RouteValues = routeValues });
        
        // Set up course service to return course with specific org ID
        var course = new CourseDto
        {
            OrganizationId = 42,
            Created = default
        };
        _courseServiceMock.Setup(s => s.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);
        
        // Execute and verify
        await _handler.HandleAsync(context);
        
        Assert.True(context.HasSucceeded);
    }
    
    [Fact]
    public async Task UserFromSameOrganization_SucceedsAuthorization()
    {
        // Setup
        var orgId = 42;
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "42"),
            new Claim(UserTokenKeys.OrganizationIds, "3"),
            new Claim(UserTokenKeys.OrganizationIds, "43") 
        }, "test"));
        
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);
        
        var courseId = Guid.NewGuid();
        var routeValues = new RouteValueDictionary { ["courseId"] = courseId.ToString() };
        
        _httpContext.User = user;
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(_httpContext);
        _httpContext.Features.Set<IRouteValuesFeature>(new RouteValuesFeature { RouteValues = routeValues });
        
        var course = new CourseDto
        {
            OrganizationId = orgId,
            Created = default
        };
        _courseServiceMock.Setup(s => s.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);
        
        await _handler.HandleAsync(context);
        
        Assert.True(context.HasSucceeded);
    }
    
    [Fact]
    public async Task UserFromSubOrganization_SucceedsAuthorization()
    {
        // Setup
        var courseOrgId = 99;
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "42"),
            new Claim(UserTokenKeys.OrganizationIds, "8"),
            new Claim(UserTokenKeys.OrganizationIds, "99")
        }, "test"));
        
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);
        
        var courseId = Guid.NewGuid();
        var routeValues = new RouteValueDictionary { ["courseId"] = courseId.ToString() };
        
        _httpContext.User = user;
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(_httpContext);
        _httpContext.Features.Set<IRouteValuesFeature>(new RouteValuesFeature { RouteValues = routeValues });
        
        var course = new CourseDto
        {
            OrganizationId = courseOrgId,
            Created = default
        };
        _courseServiceMock.Setup(s => s.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);
        
        await _handler.HandleAsync(context);
        
        Assert.True(context.HasSucceeded);
    }
    
    [Fact]
    public async Task UserFromDifferentOrganization_FailsAuthorization()
    {
        // Setup
        var courseOrgId = 99;
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "42"),
            new Claim(UserTokenKeys.OrganizationIds, "8"),
            new Claim(UserTokenKeys.OrganizationIds, "100")
        }, "test"));
        
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);
        
        var courseId = Guid.NewGuid();
        var routeValues = new RouteValueDictionary { ["courseId"] = courseId.ToString() };
        
        _httpContext.User = user;
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(_httpContext);
        _httpContext.Features.Set<IRouteValuesFeature>(new RouteValuesFeature { RouteValues = routeValues });
        
        var course = new CourseDto
        {
            OrganizationId = courseOrgId,
            Created = default
        };
        _courseServiceMock.Setup(s => s.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);
        
        await _handler.HandleAsync(context);
        
        Assert.True(context.HasFailed);
    }
}
