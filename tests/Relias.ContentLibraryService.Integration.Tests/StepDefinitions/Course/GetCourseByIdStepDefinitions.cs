using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using System.Net;
using System.Net.Http.Json;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]

public class GetCourseByIdStepDefinitions(
    HttpResponseContext httpResponseContext,
    AuthenticationHeaderContext authenticationHeaderContext,
    ScenarioService scenarioService)
{
    private readonly string _endpointUri = "api/v1/courses";
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
    private readonly Guid _contentId = Guid.NewGuid();
    private readonly Guid _validCourseId = GlobalTestSetup.AccessCourseOrgCourseIds[0];
    private readonly Guid _validCourseIdWrongOrg = GlobalTestSetup.AccessCourseOrgCourseIds[1];

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }


    [Given("the user has access to a course")]
    public async Task GivenTheUserHasAccessToACourse()
    {
        var context = GlobalTestSetup.CreateContext();

        var contentType = new Domain.ContentType.ContentType
        {
            ContentTypeDescription = "Content Type Test"
        };
        context.ContentType.Add(contentType);
        await context.SaveChangesAsync();

        var content = new Domain.Content.Content
        {
            ContentId = _contentId,
            ContentTypeId = contentType.ContentTypeId
        };

        context.Content.AddRange(content);
        await context.SaveChangesAsync();

        var course = new Domain.Course.Course
        {
            CourseId = Guid.NewGuid(),
            OrganizationId = 8,
            ContentId = _contentId,
            ContentCode = "C001",
            Title = "Introduction to Programming",
            Description = "This course provides an introduction to programming concepts using Python.",
            BriefDescription = "Introductory programming with Python.",
            StatusId = 1,
            Created = DateTime.Now,
            CreatedBy = Guid.NewGuid().ToString(),
            LastModified = DateTime.Now,
            LastModifiedBy = Guid.NewGuid().ToString()
        };

        context.Courses.Add(course);
        await context.SaveChangesAsync();
    }

    [When("the user access an existing course")]
    public async Task WhenTheUserAccessAnExistingCourse()
    {
        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}/{_validCourseId}");
    }

    [When("the user access a course that does not exist")]
    public async Task WhenTheUserAccessACourseThatDoesNotExist()
    {
        var courseId = Guid.NewGuid();

        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}/{courseId}");
    }

    [When("the user requests a course from an organization they do not belong to")]
    public async Task WhenTheUserRequestsACourseFromAnOrganizationTheyDoNotBelongTO()
    {
        _client.SetAuthHeaderWithOrg(TestOrgIds.NoAccessOrg, TestOrgIds.NoAccessOrg);
        
        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}/{_validCourseIdWrongOrg}");
    }

    [When("the user access an invalid course ID")]
    public async Task WhenTheUserAccessAnInvalidCourseId()
    {
        var courseId = 12345;

        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}/{courseId}");
    }

    [When("the user does not provide a course ID")]
    public async Task WhenTheUserDoesNotProvideACourseId()
    {
        var courseId = Guid.NewGuid();

        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}/{courseId}");
    }

    [Then("a course is returned")]
    public async Task ThenACourseIsReturned()
    {
        var response = await httpResponseContext.Response!.Content.ReadFromJsonAsync<CourseDto>();

        Assert.NotNull(response);
    }

    [Then("no course should be returned")]
    public void ThenNoCourseShouldBeReturned()
    {
        var statusCode = httpResponseContext.Response?.StatusCode;

        Assert.Equal(HttpStatusCode.NotFound, statusCode);
    }

    [When("the unauthenticated user requests to access an existing course")]
    public async Task WhenTheUnauthenticatedUserRequestsACourseByIdAsync()
    {
        var courseId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = null;

        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}/{courseId}");
    }
}

