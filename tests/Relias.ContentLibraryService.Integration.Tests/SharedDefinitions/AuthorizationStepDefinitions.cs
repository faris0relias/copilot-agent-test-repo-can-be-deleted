using Relias.ContentLibraryService.Integration.Tests.Context;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.SharedDefinitions;

[Binding]
public sealed class AuthorizationStepDefinitions(AuthenticationHeaderContext authenticationHeaderContext)
{
    [Given("An unauthorized user")]
    [Given("an unauthorized user")]
    [Given("the user is not authenticated")]
    public void GivenAnUnauthorizedUser()
    {
        authenticationHeaderContext.AuthenticationHeader = null;
    }
}