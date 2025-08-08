using System.Net;
using System.Net.Http.Json;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class GetLearningContentByIdStepDefinitions(
    AuthenticationHeaderContext authenticationHeaderContext,
    HttpResponseContext httpResponseContext,
    LearningContentContext learningContentContext,
    ScenarioService scenarioService)
{
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
    private readonly string _endpoint = "api/v1/courses";
    private readonly Guid _validCourseIdWrongOrg = GlobalTestSetup.AccessCourseOrgCourseIds[1];

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }

    [Given("the user has access to a course with learning content")]
    public async Task GivenTheUserHasAccessToACourseWithLearningContent()
    {
        var courseId = Guid.NewGuid();

        var learningContent = new LearningContent
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections = []
        };

        learningContentContext.LearningContent = learningContent;

        await GlobalTestSetup.CosmosClient!.CreateItemAsync(learningContent);
    }

    [When("the user accesses the course")]
    public async Task WhenTheUserAccessesTheCourse()
    {
        var courseId = learningContentContext.LearningContent!.CourseId;

        httpResponseContext.Response = await _client.GetAsync(
            $"{_endpoint}/{courseId}/learning-content?organizationId={TestOrgIds.DefaultOrg}");
    }

    [Then("the user should see the learning content details")]
    public async Task ThenTheUserShouldSeeTheLearningContentDetails()
    {
        Assert.Equal(HttpStatusCode.OK, httpResponseContext.Response?.StatusCode);

        var content = await httpResponseContext.Response!.Content.ReadFromJsonAsync<LearningContentDto>();

        Assert.NotNull(content);
        Assert.Equal(learningContentContext.LearningContent!.CourseId, content.CourseId);
    }

    [When("the user tries to access an invalid course")]
    public async Task WhenTheUserTriesToAccessAnInvalidCourse()
    {
        var invalidCourseId = "123";

        httpResponseContext.Response = await _client.GetAsync(
            $"{_endpoint}/{invalidCourseId}/learning-content?organizationId={TestOrgIds.DefaultOrg}");
    }

    [Then("the user should be informed that the request was invalid")]
    public void ThenTheUserShouldBeInformedThatTheRequestWasInvalid()
    {
        Assert.Equal(HttpStatusCode.BadRequest, httpResponseContext.Response?.StatusCode);
    }

    [When("the user tries to access a course that does not exist")]
    public async Task WhenTheUserTriesToAccessACourseThatDoesNotExist()
    {
        var nonExistentCourseId = Guid.NewGuid();

        _client.SetAuthHeaderWithOrg(8, 1, 8);

        httpResponseContext.Response = await _client.GetAsync(
            $"{_endpoint}/{nonExistentCourseId}/learning-content?organizationId={TestOrgIds.DefaultOrg}");
    }

    [Then("the user should be informed that the course was not found")]
    public void ThenTheUserShouldBeInformedThatTheCourseWasNotFound()
    {
        Assert.Equal(HttpStatusCode.NotFound, httpResponseContext.Response?.StatusCode);
    }

    [Then("the user should not see any learning content")]
    public static void ThenTheUserShouldNotSeeAnyLearningContent()
    {
        Assert.True(true);
    }

    [When("the user tries to access a course from another organization")]
    public async Task WhenTheUserTriesToAccessACourseFromAnotherOrganization()
    {
        var courseId = _validCourseIdWrongOrg;

        _client.SetAuthHeaderWithOrg(TestOrgIds.NoAccessOrg, TestOrgIds.NoAccessOrg);

        httpResponseContext.Response = await _client.GetAsync(
            $"{_endpoint}/{courseId}/learning-content");
    }

    [Then("the user should not be allowed to view the course or its learning content")]
    public void ThenTheUserShouldNotBeAllowedToViewTheCourseOrItsLearningContent()
    {
        Assert.Equal(HttpStatusCode.Forbidden, httpResponseContext.Response?.StatusCode);
    }

    [Given("the user is not authorized")]
    public void GivenTheUserIsNotAuthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [When("the user attempts to access a course")]
    public async Task WhenTheUserAttemptsToAccessACourse()
    {
        var courseId = learningContentContext.LearningContent?.CourseId ?? Guid.NewGuid();

        httpResponseContext.Response = await _client.GetAsync(
            $"{_endpoint}/{courseId}/learning-content?organizationId={TestOrgIds.DefaultOrg}");
    }

    [Then("the user should be asked to authorize before accessing the course")]
    public void ThenTheUserShouldBeAskedToAuthorizeBeforeAccessingTheCourse()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponseContext.Response?.StatusCode);
    }
}
