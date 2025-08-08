using System;
using System.Net.Http.Json;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions
{
    [Binding]
    public class DeleteLessonStepDefinitions(
        HttpResponseContext httpResponseContext,
        AuthenticationHeaderContext authenticationHeaderContext,
        ScenarioService scenarioService)
    {
        private const string EndpointUri = "api/v1/courses";
        private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
        private readonly Guid _validCourseId = GlobalTestSetup.UpdateCourseIds[5];
        private readonly CancellationToken _cancellationToken = new();
        private readonly List<LearningContent> _learningContent = GlobalTestSetup.LearningContent;
        private Guid _sectionId;
        private Guid _learningObjectId;

        [BeforeStep]
        public void SetDefaultAuthorizationHeader()
        {
            if (authenticationHeaderContext.AuthenticationHeader is not null)
            {
                _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
            }
        }

        [Given("the user is authorized to delete lessons")]
        public void GivenTheUserIsAuthorizedToDeleteLessons()
        {
            var authHeaderValue = IntegrationTestAuthHelper.GenerateAuthHeaderValueFromClaims(
                [
                    new(UserTokenKeys.Subject, "100"),
                    new(UserTokenKeys.UserId, "100"),
                    new(UserTokenKeys.ClientId, "platform-integration-tests"),
                    new(UserTokenKeys.Permissions, "32"),
                    new(UserTokenKeys.OrganizationIds, "1"),
                    new(UserTokenKeys.OrganizationIds, "8")
                ]);

            authenticationHeaderContext.AuthenticationHeader = authHeaderValue;
        }

        [When("the user requests to delete a lesson")]
        public async Task WhenTheUserRequestsToDeleteALesson()
        {
            LearningContent? learningContent = _learningContent.Find(lc => lc.CourseId == _validCourseId);
            _sectionId = learningContent?.Sections.Find(s => s.LearningObjects.Count > 0)?.SectionId ?? throw new InvalidOperationException("No section with learning objects found");
            _learningObjectId = learningContent.Sections.Find(s => s.SectionId == _sectionId)?.LearningObjects.First()
                ?.LearningObjectId ?? throw new InvalidOperationException("No learning object found in section");

            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{_validCourseId}/learning-content/section/{_sectionId}/lesson/{_learningObjectId}", _cancellationToken);
        }

        [Then("the lesson should be removed from the course")]
        public async Task ThenTheLessonShouldBeRemovedFromTheCourse()
        {
            httpResponseContext.Response = await _client.GetAsync($"{EndpointUri}/{_validCourseId}/learning-content?organizationId={TestOrgIds.DefaultOrg}");
            var content = await httpResponseContext.Response!.Content.ReadFromJsonAsync<LearningContentDto>();

            Assert.NotNull(content);
            LearningContentSectionDto? section = content.Sections.Find(s => s.SectionId == _sectionId);
            Assert.NotNull(section);
            LearningObjectDto? learningObject = section.LearningObjects.Find(lo => lo.LearningObjectId == _learningObjectId);
            Assert.Null(learningObject);
        }

        [When("the user requests to delete a lesson without a valid lesson ID")]
        public async Task WhenTheUserRequestsToDeleteALessonWithoutAValidLessonID()
        {
            LearningContent? learningContent = _learningContent.Find(lc => lc.CourseId == _validCourseId);
            _sectionId = learningContent?.Sections.Find(s => s.LearningObjects.Count > 0)?.SectionId ?? throw new InvalidOperationException("No section with learning objects found");
            
            var invalidLessonId = Guid.Empty;
            
            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{_validCourseId}/learning-content/section/{_sectionId}/lesson/{invalidLessonId}", _cancellationToken);
        }

        [When("the user requests to delete a lesson without a valid course ID")]
        public async Task WhenTheUserRequestsToDeleteALessonWithoutAValidCourseID()
        {
            LearningContent? learningContent = _learningContent.Find(lc => lc.CourseId == _validCourseId);
            _sectionId = learningContent?.Sections.Find(s => s.LearningObjects.Count > 0)?.SectionId ?? throw new InvalidOperationException("No section with learning objects found");
            _learningObjectId = learningContent.Sections.Find(s => s.SectionId == _sectionId)?.LearningObjects.First()
                ?.LearningObjectId ?? throw new InvalidOperationException("No learning object found in section");

            var invalidCourseId = Guid.Empty;
            
            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{invalidCourseId}/learning-content/section/{_sectionId}/lesson/{_learningObjectId}", _cancellationToken);
        }

        [When("the user requests to delete a lesson that does not exist with a valid course ID")]
        public async Task WhenTheUserRequestsToDeleteALessonThatDoesNotExistWithAValidCourseID()
        {
            LearningContent? learningContent = _learningContent.Find(lc => lc.CourseId == _validCourseId);
            _sectionId = learningContent?.Sections.Find(s => s.LearningObjects.Count > 0)?.SectionId ?? throw new InvalidOperationException("No section with learning objects found");
            
            _learningObjectId = Guid.NewGuid();
            
            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{_validCourseId}/learning-content/section/{_sectionId}/lesson/{_learningObjectId}", _cancellationToken);
        }

        [When("the user requests to delete a lesson that does not exist with a section ID")]
        public async Task WhenTheUserRequestsToDeleteALessonThatDoesNotExistWithASectionID()
        {
            LearningContent? learningContent = _learningContent.Find(lc => lc.CourseId == _validCourseId);
            _learningObjectId = learningContent?.Sections.Find(s => s.LearningObjects.Count > 0)?.LearningObjects.First()
                ?.LearningObjectId ?? throw new InvalidOperationException("No learning object found");
            
            _sectionId = Guid.NewGuid();
            
            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{_validCourseId}/learning-content/section/{_sectionId}/lesson/{_learningObjectId}", _cancellationToken);
        }

        [When("the unauthenticated user requests to delete a lesson")]
        public async Task WhenTheUnauthenticatedUserRequestsToDeleteALesson()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            
            LearningContent? learningContent = _learningContent.Find(lc => lc.CourseId == _validCourseId);
            _sectionId = learningContent?.Sections.Find(s => s.LearningObjects.Count > 0)?.SectionId ?? throw new InvalidOperationException("No section with learning objects found");
            _learningObjectId = learningContent.Sections.Find(s => s.SectionId == _sectionId)?.LearningObjects.First()
                ?.LearningObjectId ?? throw new InvalidOperationException("No learning object found in section");

            httpResponseContext.Response = await _client.DeleteAsync($"{EndpointUri}/{_validCourseId}/learning-content/section/{_sectionId}/lesson/{_learningObjectId}", _cancellationToken);
        }

    }
}
