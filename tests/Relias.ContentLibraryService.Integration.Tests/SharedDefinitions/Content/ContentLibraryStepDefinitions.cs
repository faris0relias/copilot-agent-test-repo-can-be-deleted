using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.SharedDefinitions.Content;

[Binding]
public class ContentLibraryStepDefinitions(AuthenticationHeaderContext authenticationHeaderContext)
{
    private readonly AuthenticationHeaderContext _authenticationHeaderContext = authenticationHeaderContext;

    [Given("An authorized user with access to content library service")]
    public void GivenAnAuthorizedUserWithAccessToContentLibrary()
    {
        var authHeaderValue = IntegrationTestAuthHelper.GenerateAuthHeaderValueFromClaims(new List<KeyValuePair<string, string>>
        {
            new(UserTokenKeys.Subject, "100"),
            new(UserTokenKeys.UserId, "100"),
            new(UserTokenKeys.ClientId, "platform-integration-tests"),
            new(UserTokenKeys.Permissions, "32")
        });

        _authenticationHeaderContext.AuthenticationHeader = authHeaderValue;
    }
}
