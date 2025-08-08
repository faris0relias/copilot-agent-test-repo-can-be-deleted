using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course
{
    [Binding]
    public class DeleteCourseStepDefinitions(
        HttpResponseContext httpResponseContext, 
        AuthenticationHeaderContext authenticationHeaderContext,
        ScenarioService scenarioService)
    {
        private const string EndpointUri = "api/v1/courses";
        private readonly ApplicationDbContext _context = GlobalTestSetup.GetAuditableEntityCompatibleContext();
        private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
        private readonly Guid _validCourseId = GlobalTestSetup.DeleteAccessOrgCourseIds[0];
        private readonly Guid _reliasOwnedCourseId = GlobalTestSetup.DeleteAccessOrgCourseIds[2];
        private readonly Guid _validCourseIdWrongOrg = GlobalTestSetup.DeleteAccessOrgCourseIds[1];
        private readonly Guid _validContentId = GlobalTestSetup.DeleteAccessOrgCourseIds[0];
        private const int DeleteCourseOrgId = TestOrgIds.DeleteCourseOrg;
        private readonly CancellationToken _cancellationToken = new();

        [BeforeStep]
        public void SetDefaultAuthorizationHeader()
        {
            if (authenticationHeaderContext.AuthenticationHeader is not null)
            {
                _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
            }
        }

        [When("the user requests to delete a course")]
        public async Task WhenTheUserRequestsToDeleteACourseAsync()
        {
            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{_validCourseId}", _cancellationToken);
        }
        
        [When("the user requests to delete a Relias owned course")]
        public async Task WhenTheUserRequestsToDeleteAReliasOwnedCourseAsync()
        {
            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{_reliasOwnedCourseId}", _cancellationToken);
        }

        [Then("the course is deleted")]
        public async Task ThenTheCourseIsDeleted()
        {
            Domain.Course.Course? course = await _context.Courses.FirstOrDefaultAsync(x => x.CourseId.Equals(_validCourseId), _cancellationToken);
            Assert.Null(course);

            Domain.Content.Content? content = await _context.Content.FirstOrDefaultAsync(x => x.ContentId.Equals(_validContentId), _cancellationToken);
            Assert.Null(content);

            var learningContent = await GlobalTestSetup.CosmosClient!.QueryFirstOrDefaultAsync<LearningContent>(q => q.Where(x => x.Id == _validCourseId), _cancellationToken);
            Assert.Null(learningContent);
        }

        [When("the user requests to delete a course without a valid course ID")]
        public async Task WhenTheUserRequestsToDeleteACourseWithoutAValidCourseId()
        {
            int invalidCourseId = 0;

            httpResponseContext.Response = await _client.DeleteAsync(
                $"{EndpointUri}/{invalidCourseId}", _cancellationToken);
        }
            
        [When("the user requests to delete a course that is not in draft status")]
        public async Task WhenTheUserRequestsToDeleteACourseThatIsNotInDraftStatus()
        {
            ApplicationDbContext context = GlobalTestSetup.GetAuditableEntityCompatibleContext();

            var archivedCourse = await context.Courses.FirstOrDefaultAsync(c => c.StatusId == 2 && c.OrganizationId == DeleteCourseOrgId, _cancellationToken);

            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{archivedCourse!.CourseId}", _cancellationToken);
        }
        
        [When("the user requests to delete a course that does not exist")]
        public async Task WhenTheUserRequestsToDeleteACourseThatDoesNotExist()
        {
            Guid nonExistingCourse = Guid.NewGuid();

            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{nonExistingCourse}", _cancellationToken);
        }
        
        [When("the user requests to delete a course from a different organization")]
        public async Task WhenTheUserRequestsToDeleteACourseFromADifferentOrganization()
        {
            _client.SetAuthHeaderWithOrg(TestOrgIds.NoAccessOrg, TestOrgIds.NoAccessOrg);
            
            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{_validCourseIdWrongOrg}", _cancellationToken);
        }

        [When("the unauthenticated user requests to delete a course")]
        public async Task WhenTheUnauthenticatedUserRequestsToDeleteACourseAsync()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{_validCourseId}", _cancellationToken);
        }
    }
}
