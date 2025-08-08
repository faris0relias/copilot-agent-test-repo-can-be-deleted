using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using System.Net.Http.Json;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class PutCourseStepDefinitions(HttpResponseContext httpResponseContext, AuthenticationHeaderContext authenticationHeaderContext, ScenarioService scenarioService)
{
    private const string EndpointUri = "api/v1/courses";
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
    private const int ValidOrgId = TestOrgIds.UpdateCourseOrg;
    private readonly Guid? _validContentId = GlobalTestSetup.UpdateContentIds[0];
    private readonly Guid? _validContentIdSecond = GlobalTestSetup.UpdateContentIds[1];
    private readonly Guid? _reliasOwnedContentId = GlobalTestSetup.UpdateContentIds[3];
    private readonly Guid _validCourseId = GlobalTestSetup.UpdateCourseIds[1];
    private readonly Guid _validCourseIdWrongOrg = GlobalTestSetup.UpdateCourseIds[2];
    private readonly Guid _reliasOwnedCourseId = GlobalTestSetup.UpdateCourseIds[4];

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }

    [When("the user provides valid updated course details")]
    public async Task WhenUserProvidesValidCourseDetails()
    {
        UpdateCourseDto updateCourse = new()
        {
            Title = "Test",
            OrganizationId = ValidOrgId,
            Description = "Test Description",
            BriefDescription = "Test Brief Description",
            LanguageIds = [Guid.NewGuid()],
            ContentCode = "VALID-NEW-COURSE-CODE-001",
            ContentId = _validContentIdSecond
        };

        var request = HttpExtensions.CreateRequestBody(updateCourse);

        httpResponseContext.Response = await _client.PutAsync($"{EndpointUri}/{_validCourseId}", request);
    }

    [Then("the course should be updated with the new values")]
    public async Task ThenCourseShouldBeUpdatedWithNewvalues()
    {
        Assert.NotNull(httpResponseContext.Response);
        var updatedCourse = await httpResponseContext.Response.Content.As<CourseDto>();
        Assert.NotNull(updatedCourse);

        Assert.True(updatedCourse.ContentId.Equals(_validContentIdSecond));     
        Assert.Equal(ValidOrgId, updatedCourse.OrganizationId);
        Assert.Equal("VALID-NEW-COURSE-CODE-001", updatedCourse.ContentCode);
        Assert.Equal("Test Description", updatedCourse.Description);
        Assert.Equal("Test Brief Description", updatedCourse.BriefDescription);
        Assert.NotEmpty(updatedCourse.LanguageIds);
    }

    [When("the user clears the content code")]
    public async Task WhenUserProvidesNoContentCode()
    {    
        UpdateCourseDto updateCourse = new()
        {
            Title = "Test",
            OrganizationId = ValidOrgId,
            Description = "Test Description",
            BriefDescription = "Test Brief Description",
            LanguageIds = [Guid.NewGuid()],
            ContentCode = "",
            ContentId = _validContentId
        };

        var request = HttpExtensions.CreateRequestBody(updateCourse);

        httpResponseContext.Response = await _client.PutAsync($"{EndpointUri}/{_validCourseId}", request);
    }

    [When("the user provides a content code longer than 100 characters")]
    public async Task WhenUserUpdateCourseWithContentCodeExceeding100CharacterLimit()
    {
        string longContentCode = new('a', 101);        
        UpdateCourseDto updateCourse = new()
        {
            Title = "Test",
            OrganizationId = ValidOrgId,
            Description = "Test Description",
            BriefDescription = "Test Brief Description",
            LanguageIds = [Guid.NewGuid()],
            ContentCode = longContentCode,
            ContentId = _validContentId
        };

        var request = HttpExtensions.CreateRequestBody(updateCourse);

        httpResponseContext.Response = await _client.PutAsync($"{EndpointUri}/{_validCourseId}", request);
    }

    [When("the user updates the content code with special characters")]
    public async Task WhenUserUpdaetCourseWithContentCodeWithSpecialCharacters()
    {    
        UpdateCourseDto updateCourse = new()
        {
            Title = "Test",
            OrganizationId = ValidOrgId,
            Description = "Test Description",
            BriefDescription = "Test Brief Description",
            LanguageIds = [Guid.NewGuid()],
            ContentCode = "INVALID.COURSE!CODE",
            ContentId = _validContentId
        };        

        var request = HttpExtensions.CreateRequestBody(updateCourse);

        httpResponseContext.Response = await _client.PutAsync($"{EndpointUri}/{_validCourseId}", request);
    }

    [When("the user updates the content code to start with special characters")]
    public async Task WhenUserUpdateCourseWithContentCodeStartingWithSpecialCharacters()
    {    
        UpdateCourseDto updateCourse = new()
        {
            Title = "Test",
            OrganizationId = ValidOrgId,
            Description = "Test Description",
            BriefDescription = "Test Brief Description",
            LanguageIds = [Guid.NewGuid()],
            ContentCode = "-INVALID-COURSE-CODE",
            ContentId = _validContentId
        };
      
        var request = HttpExtensions.CreateRequestBody(updateCourse);

        httpResponseContext.Response = await _client.PutAsync($"{EndpointUri}/{_validCourseId}", request);
    }
    
    [When("the user attempts to update a Relias owned course")]
    public async Task WhenUserAttemptsToUpdateAReliasOwnedCourse()
    {    
 
        UpdateCourseDto updateCourse = new()
        {
            Title = "Test Relias Course",
            OrganizationId = ValidOrgId,
            Description = "Test Description",
            BriefDescription = "Test Brief Description",
            LanguageIds = [Guid.NewGuid()],
            ContentCode = "REL-MOCK-123",
            ContentId = _reliasOwnedContentId
        };

        JsonContent request = HttpExtensions.CreateRequestBody(updateCourse);

        httpResponseContext.Response = await _client.PutAsync($"{EndpointUri}/{_reliasOwnedCourseId}", request);
    }

    [When("the user tries to update the current course with the same content code")]
    public async Task WhenUserProvidesExistingContentCode()
    {     
        var validSecondContentId = GlobalTestSetup.UpdateCourseIds[1];
        UpdateCourseDto updateCourse = new()
        {
            Title = "Test",
            OrganizationId = ValidOrgId,
            Description = "Test Description",
            BriefDescription = "Test Brief Description",
            LanguageIds = [Guid.NewGuid()],
            ContentCode = "C101",
            ContentId = validSecondContentId
        };

        var request = HttpExtensions.CreateRequestBody(updateCourse);
        httpResponseContext.Response = await _client.PutAsync($"{EndpointUri}/{_validCourseId}", request);
    }
    
    [When("the user does not have the required permissions to edit")]
    public async Task WhenUserMissingRequiredPermission()
    {       
        UpdateCourseDto updateCourse = new()
        {
            Title = "Test",
            OrganizationId = TestOrgIds.Site1Org,
            Description = "Test Description",
            BriefDescription = "Test Brief Description",
            LanguageIds = [Guid.NewGuid()],
            ContentCode = "-INVALID-COURSE-CODE",
            ContentId = _validContentId
        };

        var request = HttpExtensions.CreateRequestBody(updateCourse);

        _client.SetAuthHeaderWithOrg(TestOrgIds.UpdateCourseOrg, TestOrgIds.UpdateCourseOrg);
        httpResponseContext.Response = await _client.PutAsync($"{EndpointUri}/{_validCourseIdWrongOrg}", request);
    }

    [When("the user is not authenticated to update")]
    public async Task WhenUserNotAuthenticated()
    {   
        UpdateCourseDto updateCourse = new()
        {
            Title = "Test",
            OrganizationId = ValidOrgId,
            Description = "Test Description",
            BriefDescription = "Test Brief Description",
            LanguageIds = [Guid.NewGuid()],
            ContentCode = "-INVALID-COURSE-CODE",
            ContentId = _validContentId
        };

        var request = HttpExtensions.CreateRequestBody(updateCourse);

        _client.DefaultRequestHeaders.Authorization = null;
        httpResponseContext.Response = await _client.PutAsync($"{EndpointUri}/{_validCourseId}", request);
    }    
}