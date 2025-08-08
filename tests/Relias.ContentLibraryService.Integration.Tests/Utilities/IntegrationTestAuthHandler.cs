using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Relias.ContentLibraryService.Integration.Tests.Utilities;

public class IntegrationTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string AuthorizationHeader = "Authorization";
    public const string TestAuthScheme = "Test";

    public IntegrationTestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeaderPresent =
            AuthenticationHeaderValue.TryParse(Request.Headers[AuthorizationHeader], out var authHeader);

        if (!authHeaderPresent)
        {
            return Task.FromResult(AuthenticateResult.Fail("User not authenticated"));
        }

        var testClaims =
            JsonSerializer.Deserialize<Dictionary<string, string>>(authHeader.Parameter!.Replace(';', ','));
        Claim[] orgIdsClaims = [];
        var claims = testClaims!.Select(kvp => new Claim(kvp.Key, kvp.Value)).ToArray();
        claims = claims.Concat(orgIdsClaims).ToArray();
        var identity = new ClaimsIdentity(claims, TestAuthScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestAuthScheme);

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}
