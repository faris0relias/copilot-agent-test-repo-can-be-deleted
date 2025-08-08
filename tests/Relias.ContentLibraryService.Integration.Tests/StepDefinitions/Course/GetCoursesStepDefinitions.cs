using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using System.Net.Http.Json;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class GetCoursesStepDefinitions(
    HttpResponseContext httpResponseContext,
    AuthenticationHeaderContext authenticationHeaderContext, 
    CourseContext courseContext, 
    ScenarioService scenarioService)
{
    private readonly string _endpointUri = "api/v1/courses/list";
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
    private readonly ApplicationDbContext _context = GlobalTestSetup.GetAuditableEntityCompatibleContext();

    private const int EmptyOrg = TestOrgIds.EmptyOrg;

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }

    [Given("no list of courses")]
    public async Task GivenNoListOfCourses()
    {
        courseContext.Courses = null;
        await _context.SaveChangesAsync();
    }


    [When("the user requests the list of available courses")]
    public async Task WhenTheUserMakesARequestToGetAListCourses()
    {
        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}?organizationId={TestOrgIds.DefaultOrg}");
    }

    [When("the user requests the list of available courses from a different organization")]
    public async Task WhenTheUserRequestsTheListOfAvailableCoursesFromADifferentOrganization()
    {
        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}?organizationId={TestOrgIds.NoAccessOrg}");
    }

    [When("the user requests the list of available courses for an organization with no course data")]
    public async Task WhenTheUserRequestsTheListOfAvailableCoursesForAnOrganizationWithNoCourseData()
    {
        _client.SetAuthHeaderWithOrg(TestOrgIds.EmptyOrg, TestOrgIds.EmptyOrg);

        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}?organizationId={TestOrgIds.EmptyOrg}");
    }

    [Then("A list of courses with its attributes should be returned")]
    public async Task ThenAListOfCoursesShouldBeReturned()
    {
        var response = await httpResponseContext.Response!.Content.ReadFromJsonAsync<CourseDto[]>();

        Assert.NotNull(response);
        Assert.NotEmpty(response);
    }

    [Then("An empty list is returned")]
    public async Task EmptyListShouldBeReturned()
    {
        var response = await httpResponseContext.Response!.Content.ReadFromJsonAsync<CourseDto[]>();
        Assert.NotNull(response);
        Assert.NotNull(response);
    }

    [Given("A user is with no organization access")]
    public void GivenAnAuthorizedUserWithAccessToCourses()
    {
        _client.SetAuthHeaderWithOrg(TestOrgIds.NoAccessOrg, TestOrgIds.NoAccessOrg);
    }

    [When("the unauthenticated user requests the list of available courses")]
    public async Task WhenTheUnauthenticatedUserRequestsToDeleteACourseAsync()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}?organizationId={TestOrgIds.DefaultOrg}");
    }
}
