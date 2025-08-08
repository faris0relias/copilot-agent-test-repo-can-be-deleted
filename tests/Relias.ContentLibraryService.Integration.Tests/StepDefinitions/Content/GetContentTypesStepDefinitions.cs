using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.Domain.ContentType;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using System.Net.Http.Json;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Content;

[Binding]
public class GetContentTypesStepDefinitions(
    HttpResponseContext httpResponseContext,
    AuthenticationHeaderContext authenticationHeaderContext)
{
    private const string EndpointUri = "api/v1/content/types";
    private readonly HttpResponseContext _httpResponseContext = httpResponseContext;
    private readonly AuthenticationHeaderContext _authenticationHeaderContext = authenticationHeaderContext;
    private readonly HttpClient _client = GlobalTestSetup.Client!;
    private readonly ApplicationDbContext _context = GlobalTestSetup.CreateContext();

    [Given("A list of content types")]
    public async Task GivenAListOfContentTypes()
    {

        var contentTypes = new List<ContentType> {
            new ContentType
            {
                ContentTypeDescription = "Content Type 1"
            },
            new ContentType
            {
                ContentTypeDescription = "Content Type 2"
            }
        };

        _context.ContentType.AddRange(contentTypes);
        await _context.SaveChangesAsync();
    }

    [When("The user access the list of Content Types")]
    public async Task WhenTheUserAccessTheListOfContentTypes()
    {
        _client.DefaultRequestHeaders.Authorization = _authenticationHeaderContext.AuthenticationHeader;
        _httpResponseContext.Response = await _client.GetAsync(EndpointUri);
    }

    [Then("A list of Content Types should be returned with values ContentTypeId and ContentTypeDescription")]
    public async Task ThenAListOfContentTypesShouldBeReturned()
    {
        var response = await _httpResponseContext.Response!.Content.ReadFromJsonAsync<ContentTypeDto[]>();

        Assert.NotNull(response);
        Assert.NotEmpty(response);
        
        foreach (var contentType in response)
        {
            Assert.True(contentType.ContentTypeId > 0, "ContentTypeId should be greater than 0.");
            Assert.False(string.IsNullOrWhiteSpace(contentType.ContentTypeDescription), "ContentTypeDescription should not be null or empty.");
        }
    }
}
