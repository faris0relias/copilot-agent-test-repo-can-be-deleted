using Microsoft.AspNetCore.Mvc;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.SharedDefinitions;

[Binding]
public sealed class ValidationStepDefinitions(HttpResponseContext httpResponseContext)
{
    [Then("the validation message should be {string}")]
    public async Task ThenValidationMessageShouldBe(string message)
    {
        var response = httpResponseContext.Response;
        Assert.NotNull(response);
        var details = await response.Content.As<ValidationProblemDetails>();
        Assert.NotNull(details);
        Assert.Single(details.Errors.Values);
        var errors = details.Errors.Values.SingleOrDefault();
        Assert.NotNull(errors);
        Assert.Single(errors);
        Assert.Equal(message, errors.SingleOrDefault());
    }
    
    [Then("the validation message should contain {string}")]
    public async Task ThenValidationMessageShouldContain(string message)
    {
        var response = httpResponseContext.Response;
        Assert.NotNull(response);
        var details = await response.Content.As<ValidationProblemDetails>();
        Assert.NotNull(details);
        Assert.Single(details.Errors.Values);
        var errors = details.Errors.Values.SingleOrDefault();
        Assert.NotNull(errors);
        Assert.Single(errors);
        Assert.Contains(message, errors.SingleOrDefault());
    }

    [Then("the error message should be {string}")]
    public async Task ThenErrorMessageShouldBe(string message)
    {
        var response = httpResponseContext.Response;
        Assert.NotNull(response);
        var details = await response.Content.As<ProblemDetails>();
        Assert.NotNull(details);
        var error = details.Detail;
        Assert.NotNull(error);
        Assert.Equal(message, error);
    }

    [Then("the error message should contain {string}")]
    public async Task ThenErrorMessageShouldContain(string message)
    {
        var response = httpResponseContext.Response;
        Assert.NotNull(response);
        var details = await response.Content.As<ProblemDetails>();
        Assert.NotNull(details);
        var error = details.Detail;
        Assert.NotNull(error);
        Assert.Contains(message, error);
    }
}