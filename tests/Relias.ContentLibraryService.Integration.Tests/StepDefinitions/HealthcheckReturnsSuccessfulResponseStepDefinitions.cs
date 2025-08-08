using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions
{
    [Binding]
    public class HealthcheckReturnsSuccessfulResponseStepDefinitions
    {
        private const string EndpointUri = "api/healthcheck";
        private readonly HttpClient _httpClient;
        private readonly HttpResponseContext _httpResponseContext;

        private HealthcheckReturnsSuccessfulResponseStepDefinitions(HttpResponseContext httpResponseContext)
        {
            _httpClient = GlobalTestSetup.Client!;
            _httpResponseContext = httpResponseContext;
        }

        [When("I check the content library service api is running")]
        public async Task WhenICheckTheAssignmentServiceApiIsRunning()
        {
            _httpResponseContext.Response = await _httpClient.GetAsync($"{EndpointUri}");
        }
    }
}
