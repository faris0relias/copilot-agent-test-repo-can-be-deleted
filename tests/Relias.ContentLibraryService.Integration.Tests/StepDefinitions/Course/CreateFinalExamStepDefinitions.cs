using System.Net;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course
{
    [Binding]
    public class CreateFinalExamStepDefinitions(
        HttpResponseContext httpResponseContext,
        AuthenticationHeaderContext authenticationHeaderContext,
        FinalExamContext finalExamContext,
        ScenarioService scenarioService)
    {
        private const string _endpointUri = "api/v1/courses";

        private readonly HttpClient _client = scenarioService.ScenarioHttpClient ??
                                              throw new ArgumentNullException(
                                                  nameof(scenarioService.ScenarioHttpClient));

        [BeforeStep]
        public void SetDefaultAuthorizationHeader()
        {
            if (authenticationHeaderContext.AuthenticationHeader is not null)
            {
                _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
            }

        }

        [When("the user creates a final exam by course ID")]
        public async Task WhenTheUserCreatesAFinalExamByCourseId()
        {   
            var requestUri = $"{_endpointUri}/{finalExamContext.CourseId}/final-exam?organizationId={TestOrgIds.DefaultOrg}";
            var requestContent = HttpExtensions.CreateRequestBody(string.Empty);
            httpResponseContext.Response = await _client.PostAsync(requestUri, requestContent);
        }

        [Then("the error returned should be {string}")]
        public async Task ThenTheErrorReturnedShouldBe(string expectedErrorMessage)
        {
            var response = httpResponseContext.Response;
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseBody);
        }
    }
}