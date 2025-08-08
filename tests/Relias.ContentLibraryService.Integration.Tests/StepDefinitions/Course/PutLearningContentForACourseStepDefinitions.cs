using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.Domain.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using System.Net.Http.Json;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course
{
    [Binding]
    public class PutLearningContentForACourseStepDefinitions(
        HttpResponseContext httpResponseContext,
        LearningContentContext learningContentContext,
        AuthenticationHeaderContext authenticationHeaderContext,
        ScenarioService scenarioService,
        ScenarioContext scenarioContext)
    {
        private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
        private readonly Guid _courseId = GlobalTestSetup.UpdateCourseIds[3];
        private readonly string _orgId = TestOrgIds.UpdateCourseOrg.ToString();
        private readonly string _endpoint = "api/v1/courses";
        private readonly string _blobContainer = "main";
        private readonly List<LearningContent> _learningContent = GlobalTestSetup.LearningContent;
        
        private static Guid _sectionId;
        private static Guid _learningObjectId;
        private static string _contentPath;
        private static UpdateLessonDto? _requestBody;
        
        private string GetRequestUri(Guid courseId, Guid learningObjectId) => $"{_endpoint}/{courseId}/learning-content/section/{_sectionId}/lesson/{learningObjectId}";

        [BeforeStep]
        public void SetDefaultAuthorizationHeader()
        {
            if (authenticationHeaderContext.AuthenticationHeader is not null)
            {
                _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
            }

        }

        [Given("learning content exists for a given section")]
        public Task GivenLearningContentExists()
        {
            LearningContent learningContent = _learningContent.Find(lc => lc.CourseId == _courseId);
            _sectionId = learningContent.Sections.Find(s => s.LearningObjects.Count > 0).SectionId;
            _learningObjectId = learningContent.Sections.Find(s => s.SectionId == _sectionId).LearningObjects.First()
                .LearningObjectId;
            _contentPath = $"{_orgId}/{_courseId}/{_learningObjectId}/";

            _requestBody = new UpdateLessonDto()
            {
                LearningObjectId = _learningObjectId,
                LearningObjectType = (int)LearningObjectType.Lesson,
                LessonType = "file",
                Name = new LocalizedStringDto { En = "Lesson 1" },
                DurationMinutes = 30,
                RequiredForCompletion = true,
                RequiresAudio = false,
                RequiresVideo = false,
                OpensInNewTab = false,
                ContentPath = _blobContainer,
                FileName = "lesson1.mp4",
                FileSize = "100",
                DeleteFile = false,
                OrgId = _orgId,
                FormatType = LessonFormatType.Video
            };

            return Task.CompletedTask;
        }

        [When("the user tries to update a lesson that does not exist")]
        public async Task WhenTheUserTriesToUpdateALessonThatDoesNotExist()
        {
            _requestBody.LearningObjectId = Guid.NewGuid();
            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);

            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }

        [When("the user updates the lesson title {string}")]
        public async Task WhenTheUserUpdatesTheLessonTitle(string title)
        {
            _requestBody!.Name.En = title;
            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);

            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }
        
        [When("the user updates the lesson type {string}")]
        public async Task WhenTheUserUpdatesTheLessonType(string lessonType)
        {
            _requestBody!.LessonType = lessonType;
            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);

            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }
        
        [When("the user updates the lesson duration to {int}")]
        public async Task WhenTheUserUpdatesTheLessonDuration(int minutes)
        {
            _requestBody!.DurationMinutes = minutes;
            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);

            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }
        
        [When("the user updates the lesson content path {string}")]
        public async Task WhenTheUserUpdatesTheLessonContentPath(string contentPath)
        {
            _requestBody!.ContentPath = contentPath;
            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);

            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }
        
        [When("the user updates the lesson completion requirement {string}")]
        public async Task WhenTheUserUpdatesTheLessonCompletionRequirement(string isRequired)
        {
            bool requiredValue = bool.Parse(isRequired);

            _requestBody!.RequiredForCompletion = requiredValue;
            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);

            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }
        
        [When("the user updates the lesson open in new tab property {string}")]
        public async Task WhenTheUserUpdatesTheLessonOpensInNewTabProperty(string value)
        {
            bool openInNewTab = bool.Parse(value);

            _requestBody!.OpensInNewTab = openInNewTab;
            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);

            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }
        
        [When("the user updates the lesson requires video property {string}")]
        public async Task WhenTheUserUpdatesTheLessonRequiresVideoProperty(string value)
        {
            bool requiresVideo = bool.Parse(value);

            _requestBody!.RequiresVideo = requiresVideo;
            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);

            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }
        
        [When("the user updates the lesson requires audio property {string}")]
        public async Task WhenTheUserUpdatesTheLessonRequiresAudioProperty(string value)
        {
            bool requiresAudio = bool.Parse(value);

            _requestBody!.RequiresAudio = requiresAudio;
            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);

            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }

        [When("the user updates the lesson to delete the file")]
        public async Task WhenTheUserUpdatesTheLessonToDeleteTheFile()
        {
            var contentPath = $"{_orgId}/{_courseId}/{_requestBody.LearningObjectId}/";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test content"));
            var blobProps = await GlobalTestSetup.MainBlobStorageRepository.PersistFileAsync(_blobContainer, contentPath, _requestBody.FileName, stream);

            _requestBody.DeleteFile = true;
            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);

            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }

        [When("the user updates the URL lesson with deleteFile set to true")]
        public async Task WhenTheUserUpdatesTheURLLessonWithDeleteFileSetToTrue()
        {
            _requestBody!.DeleteFile = true;
            _requestBody.ContentPath = "https://some-awesome-url.com/resource";
            _requestBody.FileName = "Some Friendly Name For An Awesome Website";
            _requestBody.LessonType = "url";
            _requestBody.FileSize = "0";
            _requestBody.FormatType = LessonFormatType.Url;
            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);

            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }

        [When("the user updates a lesson")]
        public async Task WhenTheUserUpdatesALesson()
        {
            string lessonType = scenarioContext["LessonType"]!.ToString()!;
            string formatTypeStr = scenarioContext["FormatType"]!.ToString()!;
            var formatType = Enum.Parse<LessonFormatType>(formatTypeStr, ignoreCase: true);

            _requestBody!.LessonType = lessonType;
            _requestBody.FormatType = formatType;

            _requestBody.ContentPath = lessonType == "url"
                ? "https://example.com/resource"
                : _blobContainer;

            _requestBody.FileName = lessonType == "url"
                ? "some-friendly-url-label"
                : "lesson1.mp4";

            _requestBody.FileSize = lessonType == "url" ? "0" : "100";
            _requestBody.OpensInNewTab = lessonType == "url";

            JsonContent requestContent = HttpExtensions.CreateRequestBody(_requestBody);
            httpResponseContext.Response = await _client.PutAsync(GetRequestUri(_courseId, _requestBody.LearningObjectId), requestContent);
        }

        [Then("the URL should remain in the content path and fileName")]
        public async Task ThenTheURLShouldRemainInTheContentPath()
        {
            Assert.NotNull(_requestBody);
            Assert.NotNull(httpResponseContext.Response);
            Assert.Equal(System.Net.HttpStatusCode.OK, httpResponseContext.Response.StatusCode);

            LessonDto? lesson = await httpResponseContext.Response!.Content.ReadFromJsonAsync<LessonDto>();
            Assert.NotNull(lesson);
            Assert.True(!string.IsNullOrEmpty(lesson.ContentPath), "Lesson ContentPath should not be empty");
            Assert.True(!string.IsNullOrEmpty(lesson.FileName), "Lesson FileName should not be empty");
            Assert.True(!string.IsNullOrEmpty(lesson.FileSize), "Lesson FileSize should not be empty");

        }

        [Then("the lesson file should be deleted")]
        public async Task ThenTheLessonFileShouldBeDeleted()
        {
            Assert.NotNull(_requestBody);
            Assert.NotNull(httpResponseContext.Response);
            Assert.Equal(System.Net.HttpStatusCode.OK, httpResponseContext.Response.StatusCode);

            LessonDto? lesson = await httpResponseContext.Response!.Content.ReadFromJsonAsync<LessonDto>();
            Assert.NotNull(lesson);        
        }

        [Then("the lesson should be updated")]
        public async Task ThenTheLessonShouldBeUpdated()
        {
            LessonDto? lesson = await httpResponseContext.Response!.Content.ReadFromJsonAsync<LessonDto>();

            Assert.NotNull(lesson);
            Assert.Equal(_requestBody.DurationMinutes, lesson.DurationMinutes);
            Assert.Equal(_requestBody.LessonType, lesson.LessonType);
            Assert.Equal(_requestBody.ContentPath, lesson.ContentPath);
            Assert.Equal(_requestBody.Name.En, lesson.Name.En);
            Assert.Equal(_requestBody.OpensInNewTab, lesson.OpensInNewTab);
            Assert.Equal(_requestBody.RequiredForCompletion, lesson.RequiredForCompletion);
            Assert.Equal(_requestBody.RequiresAudio, lesson.RequiresAudio);
            Assert.Equal(_requestBody.RequiresVideo, lesson.RequiresVideo);
            Assert.Equal(_requestBody.FormatType, lesson.FormatType);
        }
    }
}
