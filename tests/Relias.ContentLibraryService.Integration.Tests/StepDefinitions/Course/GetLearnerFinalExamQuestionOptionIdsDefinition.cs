using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.FinalExam;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course
{

    [Binding]
    public class GetLearnerFinalExamQuestionOptionIdsDefinition(
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

       


        [When("the learner accesses an existing final exam")]
        public async Task WhenTheUserAccessesAnExistingFinalExam()
        {
            httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}{finalExamContext.FinalExam?.CourseId}/learner/final-exam/question-answer-ids");
        }

        [When("the learner accesses a final exam that doesn't exist")]
        public async Task WhenTheUserAccessesAFinalExamThatDoesNotExist()
        {
            var courseId = Guid.NewGuid();

            httpResponseContext.Response = await _client.GetAsync($"{_endpointUri}{courseId}/learner/final-exam/question-answer-ids");
        }

        [Then("a learner final exam is returned")]
        public void ThenAFinalExamIsReturned()
        {
            var response = httpResponseContext.Response!.Content;

            Assert.NotNull(response);
        }
        
    }
}
