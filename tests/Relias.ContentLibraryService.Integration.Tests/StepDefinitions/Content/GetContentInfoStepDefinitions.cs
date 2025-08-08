using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Content;

[Binding]
public class GetContentInfoStepDefinitions(
    HttpResponseContext httpResponseContext,
    AuthenticationHeaderContext authenticationHeaderContext)
{
    private const string EndpointUri = "api/v1/content/info";
    private readonly HttpResponseContext _httpResponseContext = httpResponseContext;
    private readonly AuthenticationHeaderContext _authenticationHeaderContext = authenticationHeaderContext;
    private readonly HttpClient _client = GlobalTestSetup.Client!;
    private readonly IEnumerable<Guid> _validContentIds = GlobalTestSetup.GetContentInfoContentIds;

    [When("The user requests content info given valid content ids")]
    public async Task WhenTheUserRequestsContentInfoGivenValidContentIds()
    {
        _client.DefaultRequestHeaders.Authorization = _authenticationHeaderContext.AuthenticationHeader;
        _client.DefaultRequestHeaders.Add("Content-Ids", string.Join(',', _validContentIds));
        _httpResponseContext.Response = await _client.GetAsync(EndpointUri);
    }

    [When("The user requests content info given invalid content ids")]
    public async Task WhenTheUserRequestsContentInfoGivenInvalidContentIds()
    {
        _client.DefaultRequestHeaders.Authorization = _authenticationHeaderContext.AuthenticationHeader;
        _client.DefaultRequestHeaders.Add("Content-Ids", $"{Guid.NewGuid()},{Guid.NewGuid()}");
        _httpResponseContext.Response = await _client.GetAsync(EndpointUri);
    }

    [When("The unauthenticated user requests a list of Content Info")]
    public async Task WhenTheUnauthenticatedUserRequestsAListOfContentInfo()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Add("Content-Ids", $"{Guid.NewGuid()},{Guid.NewGuid()}");
        _httpResponseContext.Response = await _client.GetAsync(EndpointUri);
    }

    [Then("A list of content information should be returned")]
    public async Task ThenAListOfContentInformationShouldBeReturned()
    {
        Assert.NotNull(_httpResponseContext.Response);
        var response = await _httpResponseContext.Response.Content.As<List<ContentInfoDto>>();

        Assert.NotNull(response);
        Assert.NotEmpty(response);
        Assert.Equal(_validContentIds.Count(), response.Count);
    }
}
