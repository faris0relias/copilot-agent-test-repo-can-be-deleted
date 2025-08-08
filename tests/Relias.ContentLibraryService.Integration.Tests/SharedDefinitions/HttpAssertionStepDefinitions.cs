using Relias.ContentLibraryService.Integration.Tests.Context;
using Reqnroll;
using System.Net;

namespace Relias.ContentLibraryService.Integration.Tests.SharedDefinitions;

[Binding]
public sealed class HttpAssertionStepDefinitions(HttpResponseContext httpResponseContext)
{
    [Then("The response status code should be {int}")]
    [Then("the response status code should be {int}")]
    public void ThenIShouldGetAResponseCodeOf(int expectedStatusCode)
    {
        Assert.Equal((HttpStatusCode)expectedStatusCode, httpResponseContext?.Response?.StatusCode);
    }

    [Then("The response status message should be {string}")]
    [Then("the response status message should be {string}")]
    public void ThenIShouldGetAResponseMessageOf(string expectedStatusMessage)
    {
        var response = httpResponseContext!.Response!.ReasonPhrase;
        Assert.Equal(expectedStatusMessage, response);
    }

    [Then("the update should be successful")]
    public void ThenTheUpdateShouldBeSuccessful()
    {
        Assert.Equal(HttpStatusCode.OK, httpResponseContext.Response?.StatusCode);
    }
}