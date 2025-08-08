using Microsoft.AspNetCore.Http.HttpResults;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.Domain.Course.Enums;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using System.Net;
using System.Net.Http.Json;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class CreateLessonStepDefinitions(
    HttpResponseContext httpResponseContext,
    AuthenticationHeaderContext authenticationHeaderContext,
    LearningContentContext learningContentContext,
    ScenarioService scenarioService,
    ScenarioContext scenarioContext)
{
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
    private const string EndpointTemplate = "api/v1/courses/{0}/learning-content/section/{1}/lesson";

    private readonly Guid _courseId = Guid.NewGuid();
    private readonly Guid _validSectionId = Guid.NewGuid();
    private Guid _sectionId = Guid.Empty;

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }

    [Given("a section already exists")]
    public async Task GivenSectionAlreadyExists()
    {
        _sectionId = _validSectionId;

        var content = new Domain.Course.LearningContent.LearningContent
        {
            Id = Guid.NewGuid(),
            CourseId = _courseId,
            Sections = [
                new()
                {
                    SectionId = _validSectionId,
                    Name = new() { En = "Section 1" },
                    LearningObjects = []
                }
            ]
        };

        learningContentContext.LearningContent = content;
        await GlobalTestSetup.CosmosClient!.CreateItemAsync(content);
    }

    [When(@"the user adds a lesson called ""(.*)"" to the section")]
    public async Task WhenUserAddsLessonWithTitle(string title)
    {
        var dto = new CreateLessonDto
        {
            LessonType = "file",
            Name = new() { En = title },
            DurationMinutes = 30,
            RequiredForCompletion = true,
            ContentPath = "some/location",
            FormatType = LessonFormatType.Video
        };

        await PostLessonAsync(dto);
    }

    [When("includes a duration of {int} minutes")]
    public async Task WhenIncludesDurationOf(int minutes)
    {
        var dto = new CreateLessonDto
        {
            LessonType = "file",
            Name = new() { En = "Lesson 1" },
            DurationMinutes = minutes,
            RequiredForCompletion = true,
            ContentPath = "some/location",
            FormatType = LessonFormatType.Video
        };

        await PostLessonAsync(dto);
    }

    [When("the user tries to add a lesson to a section but leaves the title blank")]
    public async Task WhenUserAddsLessonWithBlankTitle()
    {
        var dto = new CreateLessonDto
        {
            LessonType = "file",
            Name = new() { En = "" },
            DurationMinutes = 30,
            RequiredForCompletion = true,
            ContentPath = "some/location",
            FormatType = LessonFormatType.Video
        };

        await PostLessonAsync(dto);
    }
    
    [When(@"the user creates a new lesson with a duration of (-?\d+) minutes")]
    public async Task WhenUserEntersNegativeDuration(int minutes)
    {
        var dto = new CreateLessonDto
        {
            LessonType = "file",
            Name = new() { En = "Lesson 1" },
            DurationMinutes = minutes,
            RequiredForCompletion = true,
            ContentPath = "some/location",
            FormatType = LessonFormatType.Video
        };

        await PostLessonAsync(dto);
    }

    [When("the user tries to add a lesson to a section that doesn't exist")]
    public async Task WhenUserAddsLessonToNonexistentSection()
    {
        _sectionId = Guid.NewGuid();

        var dto = new CreateLessonDto
        {
            LessonType = "file",
            Name = new() { En = "Lesson 1" },
            DurationMinutes = 20,
            RequiredForCompletion = true,
            ContentPath = "some/location",
            FormatType = LessonFormatType.Video
        };

        await PostLessonAsync(dto);
    }

    [When("the user adds a new lesson")]
    public async Task WhenUserAddsNewLesson()
    {
        var lessonType = scenarioContext["LessonType"]!.ToString();
        var formatType = scenarioContext["FormatType"]!.ToString();

        var dto = new CreateLessonDto
        {
            LessonType = lessonType,
            FormatType = Enum.Parse<LessonFormatType>(formatType, ignoreCase: true),
            Name = new() { En = $"Lesson for {formatType}" },
            DurationMinutes = 15,
            RequiredForCompletion = true,
            ContentPath = lessonType == "url" ? "https://example.com/resource" : "some/location",
            OpensInNewTab = lessonType == "url" ? true : null
        };

        await PostLessonAsync(dto);
    }
    
    private async Task PostLessonAsync(CreateLessonDto dto)
    {
        var uri = string.Format(EndpointTemplate, _courseId, _sectionId);
        var request = HttpExtensions.CreateRequestBody(dto);
        httpResponseContext.Response = await _client.PostAsync(uri, request);
    }

    [Then("the lesson should be added successfully")]
    public async Task ThenLessonShouldBeAddedSuccessfully()
    {
        Assert.Equal(HttpStatusCode.OK, httpResponseContext.Response?.StatusCode);
        var lesson = await httpResponseContext.Response!.Content.ReadFromJsonAsync<LessonDto>();

        Assert.NotNull(lesson);
        Assert.NotEqual(Guid.Empty, lesson.LearningObjectId);
    }

    [Then("the lesson should not be added")]
    public void ThenLessonShouldNotBeAdded()
    {
        Assert.NotEqual(HttpStatusCode.OK, httpResponseContext.Response?.StatusCode);
    }
}
