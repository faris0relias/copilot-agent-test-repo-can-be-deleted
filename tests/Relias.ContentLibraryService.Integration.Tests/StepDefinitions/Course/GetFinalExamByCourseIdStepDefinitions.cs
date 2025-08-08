using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class GetFinalExamByCourseIdStepDefinitions(
    HttpResponseContext httpResponseContext,
    AuthenticationHeaderContext authenticationHeaderContext,
    FinalExamContext finalExamContext,
    ScenarioService scenarioService)
{
    private readonly string _endpointUri = "api/v1/courses/";
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }

    [When("the user accesses an existing final exam")]
    public async Task WhenTheUserAccessesAnExistingFinalExam()
    {
        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}{finalExamContext.FinalExam?.CourseId}/final-exam");
    }

    [When("the user accesses a final exam that doesn't exist")]
    public async Task WhenTheUserAccessesAFinalExamThatDoesNotExist()
    {
        var courseId = Guid.NewGuid();

        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}{courseId}/final-exam");
    }

    [Then("a final exam is returned")]
    public void ThenAFinalExamIsReturned()
    {
        var response = httpResponseContext.Response!.Content;

        Assert.NotNull(response);
    }
}