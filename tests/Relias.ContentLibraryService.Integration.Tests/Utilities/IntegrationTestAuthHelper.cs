using System.Net.Http.Headers;
using System.Text.Json;
using static Relias.ContentLibraryService.Common.Helpers.PermissionHelper;

namespace Relias.ContentLibraryService.Integration.Tests.Utilities;

public static class IntegrationTestAuthHelper
{
    public static AuthenticationHeaderValue GenerateAuthHeaderValueFromClaims(
        IEnumerable<KeyValuePair<string, string>>? overrideClaims = null)
    {
        var defaultClaims = new List<KeyValuePair<string, string>>
        {
            new(UserTokenKeys.Subject, "100"),
            new(UserTokenKeys.OrganizationId, "8"),
            new(UserTokenKeys.Permissions, $"{Enum.GetValues<LegacyPermission>().Sum(x => (int)x)}")
        };

        var combinedClaims = new List<KeyValuePair<string, string>>(defaultClaims);

        if (overrideClaims != null)
        {
            foreach (var claim in overrideClaims)
            {
                combinedClaims.RemoveAll(c => c.Key == claim.Key);
                combinedClaims.Add(claim);
            }
        }

        var claimsDictionary = combinedClaims
            .GroupBy(claim => claim.Key)
            .ToDictionary(
                group => group.Key,
                group => string.Join(",", group.Select(claim => claim.Value))
            );

        var parameter = JsonSerializer.Serialize(claimsDictionary).Replace(',', ';');
        return new AuthenticationHeaderValue(IntegrationTestAuthHandler.TestAuthScheme, parameter);
    }

    public static string CreateOrgIdsClaimStringFromList(List<int> orgIds)
    {
        var orgIdsAsStrings = orgIds.Select(o => o.ToString());
        return JsonSerializer.Serialize(orgIdsAsStrings);
    }
}
