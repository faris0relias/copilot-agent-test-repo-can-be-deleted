using Relias.ContentLibraryService.Domain.Course.FinalExam;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class GetLearnerFinalExamSettingByCourseIdStepDefinition(
    HttpResponseContext httpResponseContext,
    AuthenticationHeaderContext authenticationHeaderContext,
    FinalExamContext finalExamContext,
    ScenarioService scenarioService)
{
    private readonly string _endpointUri = "api/v1/courses/";
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
    private readonly Guid examId = Guid.NewGuid();
    private readonly Guid courseId = GlobalTestSetup.PatchCourseIds[0];
    
    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }

    [Given("learner final exam setting exists")]
    public async Task GivenLearnerFinalExamSettingExists()
    {
        var finalExam = new FinalExam
        {
            Id = examId.ToString(),
            CourseId = courseId,
            Created = DateTime.UtcNow,
            MinimumPercentageToPass = 70,
            QuestionsDisplayedPerExam = 10,
            Duration = new Duration 
            {
                Hours = 1,
                Minutes = 30
            }
        };

        finalExamContext!.FinalExam = finalExam;
        await GlobalTestSetup.CosmosClient!.CreateItemAsync(finalExam);
    }


    [When("the learner accesses the final exam settings")]
    public async Task  WhenTheLearnerAccessesTheFinalExamSettings()
    {
        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}{finalExamContext.FinalExam?.CourseId}/learner/final-exam/settings");
    }

    [When("the learner start final exam")]
    public async Task WhenTheLearnerStartFinalExam()
    {
        httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}{courseId}/learner/final-exam/settings");
    }


    [Then("final exam setting should be returned")]
    public void ThenAFinalExamSetting()
    {
        var response = httpResponseContext.Response!.Content;

        Assert.NotNull(response);
    }
}