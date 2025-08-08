using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using System.Net.Http.Json;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class UpdateFinalExamStepDefinitions(
        HttpResponseContext httpResponseContext,
        AuthenticationHeaderContext authenticationHeaderContext,
        FinalExamContext finalExamContext,
        ScenarioService scenarioService)
{
    private const string _endpointUri = "api/v1/courses";
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient!;
    private Dictionary<string, dynamic>? _providedValues;
    private FinalExamDto? _updatedDto;

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }

    [When("the user makes a request to update a final exam")]
    public async Task WhenTheUserMakesARequestToUpdateAFinalExam()
    {
        var request = HttpExtensions.CreateRequestBody(_providedValues);
        httpResponseContext.Response =
            await _client.PatchAsync($"{_endpointUri}/{finalExamContext.CourseId}/final-exam?organizationId={TestOrgIds.DefaultOrg}", request);
    }

    [When("provided input has valid values")]
    public void WhenProvidedInputHasValidValues()
    {
        _providedValues = new Dictionary<string, dynamic>()
        {
            { "minimumPercentageToPass", 50 }
        };

        _updatedDto = new FinalExamDto()
        {
            MinimumPercentageToPass = 50,
            QuestionsDisplayedPerExam = null,
            Duration = null
        };
    }

    [Then("the final exam should be updated")]
    public async Task ThenTheFinalExamShouldBeUpdated()
    {
        var updatedExam = await httpResponseContext.Response!.Content.ReadFromJsonAsync<FinalExamDto>();
        Assert.Equal(_updatedDto?.MinimumPercentageToPass, updatedExam!.MinimumPercentageToPass);
        Assert.Null(updatedExam.QuestionsDisplayedPerExam);
        Assert.Null(updatedExam.Duration);

    }
}