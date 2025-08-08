using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Content;

[Binding]
public class GetLanguagesStepDefinitions(
    HttpResponseContext httpResponseContext,
    AuthenticationHeaderContext authenticationHeaderContext)
{
    private const string EndpointUri = "api/v1/content/languages";
    private readonly HttpResponseContext _httpResponseContext = httpResponseContext;
    private readonly AuthenticationHeaderContext _authenticationHeaderContext = authenticationHeaderContext;
    private readonly HttpClient _client = GlobalTestSetup.Client ?? throw new ArgumentNullException(nameof(_client));

    [Given("A list of languages")]
    public static void GivenAListOfLanguages()
    {
        ApplicationDbContext context = GlobalTestSetup.CreateContext();

        Assert.True(context.Languages.Any(), "Expected Language data to exist in the database.");
    }

    [When("The user makes a request to get a list of Languages")]
    public async Task WhenTheUserMakesARequestToGetAListOfLanguages()
    {
        _client.DefaultRequestHeaders.Authorization = _authenticationHeaderContext.AuthenticationHeader;
        _httpResponseContext.Response = await _client.GetAsync(EndpointUri);
    }

    [Then("A list of Language objects should be returned with values LanguageId Code and Name")]
    public async Task ThenAListOfLanguagesShouldBeReturned()
    {
        Assert.NotNull(_httpResponseContext.Response);
        var response = await _httpResponseContext.Response.Content.As<List<LanguageDto>>();

        Assert.NotNull(response);
        Assert.NotEmpty(response);
        Assert.Equal(2, response.Count);

        foreach (var language in response)
        {
            Assert.True(Guid.TryParse(language.LanguageId.ToString(), out _), "Language should be a valid Guid.");
            Assert.False(string.IsNullOrWhiteSpace(language.Code), "Code should not be null or empty.");
            Assert.False(string.IsNullOrWhiteSpace(language.Name), "Name should not be null or empty.");
        }
    }
}
