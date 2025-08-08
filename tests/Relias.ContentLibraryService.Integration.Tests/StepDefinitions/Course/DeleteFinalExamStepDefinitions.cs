using Moq;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.FinalExam;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using StackExchange.Redis;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class DeleteFinalExamStepDefinitions(
        HttpResponseContext httpResponseContext,
        AuthenticationHeaderContext authenticationHeaderContext,
        FinalExamContext finalExamContext,
        ScenarioService scenarioService)
{
    private const string _endpointUri = "api/v1/courses";
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }

    [When("the user requests to delete a final exam")]
    public async Task WhenTheUserRequestsToDeleteAFinalExamAsync()
    {
        httpResponseContext.Response = await _client.DeleteAsync(
        $"{_endpointUri}/{finalExamContext.CourseId}/final-exam");
    }

    [When("the user requests to delete a final exam that does not exist")]
    public async Task WhenTheUserRequestsToDeleteAFinalExamThatDoesNotExistAsync()
    {
        var testCourseId = Guid.NewGuid();
        httpResponseContext.Response = await _client.DeleteAsync(
        $"{_endpointUri}/{testCourseId}/final-exam");
    }
}