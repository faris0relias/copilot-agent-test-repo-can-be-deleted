namespace Relias.ContentLibraryService.Api.Utilities;

using Authorization.Exceptions;
using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static int? OrganizationId(this ClaimsPrincipal user)
    {
        return int.TryParse(user.FindFirstValue(UserTokenKeys.OrganizationId), out var orgId)
            ? orgId
            : throw new ClaimsAccessorException($"Unable to determine Org Id from current HTTP Context");
    }

    public static IEnumerable<int> OrganizationIds(this ClaimsPrincipal user)
    {
        return user.FindAll(UserTokenKeys.OrganizationIds).Select(claim => int.Parse(claim.Value));
    }
}

public static class UserTokenKeys
{
    public const string OrganizationId = "org_id";
    public const string OrganizationIds = "org_ids";
}