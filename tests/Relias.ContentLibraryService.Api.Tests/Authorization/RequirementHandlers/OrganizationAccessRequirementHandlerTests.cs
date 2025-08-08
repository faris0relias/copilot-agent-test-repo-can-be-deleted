using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Relias.ContentLibraryService.Api.Authorization;
using Relias.ContentLibraryService.Api.Authorization.RequirementHandlers;
using Relias.ContentLibraryService.Api.Utilities;
using System.Security.Claims;
using System.Text;

namespace Relias.ContentLibraryService.Api.Tests.Authorization.RequirementHandlers;

public class OrganizationAccessRequirementHandlerTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<OrganizationAccessRequirementHandler>> _loggerMock;
    private readonly OrganizationAccessRequirementHandler _handler;
    private readonly DefaultHttpContext _httpContext;

    public OrganizationAccessRequirementHandlerTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<OrganizationAccessRequirementHandler>>();

        _handler = new OrganizationAccessRequirementHandler(
            _httpContextAccessorMock.Object,
            _loggerMock.Object
        );

        _httpContext = new DefaultHttpContext();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenHttpContextIsNull_FailsAuthorization()
    {
        var context = new AuthorizationHandlerContext(
            new[] { new OrganizationAccessRequirement() },
            new ClaimsPrincipal(),
            null);

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsNotAuthenticated_FailsAuthorization()
    {
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "2")
        }));

        _httpContext.User = userClaims;

        var context = new AuthorizationHandlerContext(
            new[] { new OrganizationAccessRequirement() },
            userClaims,
            null);

        _httpContext.Request.RouteValues[ReliasRequestValues.OrganizationId] = "2";
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContext);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserHasAccess_SucceedsAuthorization()
    {
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "2"),
            new Claim(UserTokenKeys.OrganizationIds, "3"),
            new Claim(UserTokenKeys.OrganizationIds, "4")
        }, "TestAuth"));

        _httpContext.User = userClaims;

        var context = new AuthorizationHandlerContext(
            new[] { new OrganizationAccessRequirement() },
            userClaims,
            null);

        _httpContext.Request.RouteValues[ReliasRequestValues.OrganizationId] = "2";
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContext);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserHasNoAccess_FailsAuthorization()
    {
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "5")
        }, "TestAuth"));

        _httpContext.User = userClaims;

        var context = new AuthorizationHandlerContext(
            new[] { new OrganizationAccessRequirement() },
            userClaims,
            null);

        _httpContext.Request.RouteValues[ReliasRequestValues.OrganizationId] = "2";
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContext);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenOrgIdFromQueryString_SucceedsAuthorization()
    {
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "3")
        }, "TestAuth"));

        _httpContext.User = userClaims;

        var context = new AuthorizationHandlerContext(
            new[] { new OrganizationAccessRequirement() },
            userClaims,
            null);

        _httpContext.Request.Query = new QueryCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { ReliasRequestValues.OrganizationId, "3" }
            });

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContext);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenOrgIdFromRequestBody_SucceedsAuthorization()
    {
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "4")
        }, "TestAuth"));

        _httpContext.User = userClaims;

        var context = new AuthorizationHandlerContext(
            new[] { new OrganizationAccessRequirement() },
            userClaims,
            null);

        var requestBody = "{\"organizationId\":\"4\"}";
        var byteArray = Encoding.UTF8.GetBytes(requestBody);
        var requestStream = new MemoryStream(byteArray);
        _httpContext.Request.Body = requestStream;
        _httpContext.Request.ContentLength = requestStream.Length;
        requestStream.Position = 0;

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContext);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenInvalidOrgIdInBody_FailsAuthorization()
    {
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(UserTokenKeys.OrganizationId, "4")
        }, "TestAuth"));

        _httpContext.User = userClaims;

        var context = new AuthorizationHandlerContext(
            new[] { new OrganizationAccessRequirement() },
            userClaims,
            null);

        var requestBody = "{\"InvalidKey\":\"4\"}";
        var byteArray = Encoding.UTF8.GetBytes(requestBody);
        var requestStream = new MemoryStream(byteArray);
        _httpContext.Request.Body = requestStream;
        _httpContext.Request.ContentLength = requestStream.Length;
        requestStream.Position = 0;

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContext);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
