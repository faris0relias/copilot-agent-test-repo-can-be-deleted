using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class PatchLearningContentStepDefinitions(
    HttpResponseContext httpResponseContext,
    LearningContentContext learningContentContext,
    AuthenticationHeaderContext authenticationHeaderContext,
    ScenarioService scenarioService)
{
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
    private const int orgId = TestOrgIds.DefaultOrg;
    private readonly Guid learningContentId = Guid.NewGuid();
    private readonly Guid _courseId = GlobalTestSetup.PatchCourseIds[0];
    private readonly Guid sectionId = Guid.NewGuid();
    private readonly Guid _secondSectionId = Guid.NewGuid();
    private readonly Guid _thirdSectionId = Guid.NewGuid();
    private readonly Guid _fourthSectionId = Guid.NewGuid();
    private readonly string _newSectionTitle = "New Section";
    private readonly Guid learningObjectId = Guid.NewGuid();
    private readonly Guid newLessonId = Guid.NewGuid();
    private readonly string _endpoint = "api/v1/courses";
    private string GetRequestUri(Guid courseId) => $"{_endpoint}/{courseId}/learning-content/section";
    private static StringContent CreateJsonPatchRequest(params object[] operations)
{
    var json = JsonSerializer.Serialize(operations);
    return new StringContent(json, Encoding.UTF8, "application/json-patch+json");
}

    private LearningContentDto? _cachedContent;

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }

    [Given("learning content exists")]
    public async Task GivenLearningContentExists()

    {
        var learningContent = new LearningContent
        {
            Id = learningContentId,

            CourseId = _courseId,
            Sections = 
            [
                new LearningContentSection
                {
                    SectionId = sectionId,
                    Name = new LocalizedString { En = "Section 1" },
                    LearningObjects = 
                    [
                        new Lesson
                        {
                            LearningObjectType = LearningObjectType.Lesson,
                            LearningObjectId = learningObjectId,
                            LessonType = "file",
                            Name = new LocalizedString { En = "Lesson 1" },
                            DurationMinutes = 30,
                            RequiredForCompletion = true,
                            RequiresAudio = false,
                            RequiresVideo = false,
                            OpensInNewTab = false,
                            ContentPath = "https://blobstorage.com/lesson1",
                            FormatType = LessonFormatType.Video
                        }
                    ]
                },
                new LearningContentSection
                {
                    SectionId = _secondSectionId,
                    Name = new LocalizedString { En = "Section 2" },
                    LearningObjects = []
                },
                new LearningContentSection
                {
                    SectionId = _thirdSectionId,
                    Name = new LocalizedString { En = "Section 3" },
                    LearningObjects = []
                },
                new LearningContentSection
                {
                    SectionId = _fourthSectionId,
                    Name = new LocalizedString { En = "Section 4" },
                    LearningObjects = []
                }
            ]
        };

        learningContentContext.LearningContent = learningContent;

        await GlobalTestSetup.CosmosClient!.CreateItemAsync(learningContent);
    }


    [When("the user updates the section title")]
    public async Task WhenTheUserUpdatesTheSectionTitle()
    {
        var requestBody = new
        {
            op = "replace", path = "/sections/0/name/en", value = "Updated Section Title" 
        };

        var requestContent = CreateJsonPatchRequest(requestBody);

        httpResponseContext.Response = await _client.PatchAsync(GetRequestUri(_courseId), requestContent);
    }
    
    [When("the user moves a section")]
    public async Task WhenTheUserMovesASection()
    {
        var requestBody = new
        {
            op = "move", from = "/sections/2", path = "/sections/1" 
        };

        var requestContent = CreateJsonPatchRequest(requestBody);

        httpResponseContext.Response = await _client.PatchAsync(GetRequestUri(_courseId), requestContent);
    }

    [Then("the section should have the updated title")]
    public async Task ThenTheSectionShouldHaveTheUpdatedTitle()
    {
        var content = await httpResponseContext.Response!.Content.ReadFromJsonAsync<LearningContentDto>();

        Assert.NotNull(content);
        Assert.Equal("Updated Section Title", content.Sections[0].Name.En);
    }

    [When("the user creates a new section")]
    public async Task WhenTheUserCreatesANewSection()
    {
        var requestBody = new
        {
            op = "add",
            path = "/sections/-",
            value = new
            {
                sectionId = Guid.NewGuid(),
                name = new
                {
                    en = _newSectionTitle
                },
                learningObjects = new object[] { }
            }
        };

        var requestContent = CreateJsonPatchRequest(requestBody);

        httpResponseContext.Response = await _client.PatchAsync(GetRequestUri(_courseId), requestContent);
    }

    [Then("a new section should be created")]
    public async Task ThenANewSectionShouldBeCreated()
    {
        _cachedContent = await httpResponseContext.Response!.Content.ReadFromJsonAsync<LearningContentDto>();

        Assert.NotNull(_cachedContent);
        Assert.NotNull(_cachedContent.Sections.Find(s => s.Name.En == _newSectionTitle));
    }

    [When("the user deletes a section")]
    public async Task WhenTheUserDeletesASection()
    {
        var requestBody = new
        {
            op = "remove",
            path = "/sections/3"
        };

        var requestContent = CreateJsonPatchRequest(requestBody);

        httpResponseContext.Response = await _client.PatchAsync(GetRequestUri(_courseId), requestContent);
    }

    [Then("the section should be deleted")]
    public async Task ThenTheSectionShouldBeDeleted()
    {
        _cachedContent = await httpResponseContext.Response!.Content.ReadFromJsonAsync<LearningContentDto>();

        Assert.NotNull(_cachedContent);
        Assert.Equal(3, _cachedContent.Sections.Count);
    }
    
    [Then("the section should be moved")]
    public async Task ThenTheSectionShouldBeMoved()
    {
        _cachedContent = await httpResponseContext.Response!.Content.ReadFromJsonAsync<LearningContentDto>();

        Assert.NotNull(_cachedContent);
        Assert.Equal("Section 3", _cachedContent.Sections[1].Name.En);
        Assert.Equal("Section 2", _cachedContent.Sections[2].Name.En);
    }

    [When("the user updates the section name with an empty string")]
    public async Task WhenTheUserUpdatesTheSectionNameWithAnEmptyString()
    {
        var requestBody = new
        {
            op = "replace",
            path = "/sections/0/name/en",
            value = string.Empty
        };

        var requestContent = CreateJsonPatchRequest(requestBody);

        httpResponseContext.Response = await _client.PatchAsync(GetRequestUri(_courseId), requestContent);
    }

    [Then("the request should be rejected")]
    public void ThenTheRequestShouldBeRejected()
    {
        Assert.Equal(HttpStatusCode.BadRequest, httpResponseContext.Response?.StatusCode);
    }

    [Then("the request should return error")]
    public void ThenTheRequestShouldReturnError()
    {
        Assert.Equal(HttpStatusCode.InternalServerError, httpResponseContext.Response?.StatusCode);
    }
}
