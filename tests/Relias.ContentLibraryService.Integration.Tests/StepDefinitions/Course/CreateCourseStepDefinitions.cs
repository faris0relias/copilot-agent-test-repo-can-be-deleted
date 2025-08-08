using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.Infra.Cosmos;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class CreateCourseStepDefinitions(
    HttpResponseContext httpResponseContext, 
    AuthenticationHeaderContext authenticationHeaderContext, 
    CourseContext courseContext, 
    ScenarioService scenarioService)
{
    private const string EndpointUri = "api/v1/courses";
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));

    private const int ValidOrgId = TestOrgIds.DefaultOrg;
    private const string ValidCourseName = "Valid Course";
    private const string ValidContentCode = "VALID-COURSE-CODE";
    private const string ExistingContentCode = "EXISTING-COURSE-CODE";

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }

    [When("the user provides valid course details")]
    public async Task WhenUserProvidesValidCourseDetails()
    {
        CreateCourseDto newCourse = new()
        {
            OrganizationId = ValidOrgId,
            CourseName = ValidCourseName,
            ContentCode = ValidContentCode
        };

        var request = HttpExtensions.CreateRequestBody(newCourse);
        
        httpResponseContext.Response = await _client.PostAsync(EndpointUri, request);
    }

    [Then("a course ID should be returned")]
    public async Task ThenCourseIdShouldBeReturned()
    {
        Assert.NotNull(httpResponseContext.Response);
        var createdCourse = await httpResponseContext.Response.Content.As<CourseDto>();
        courseContext.CreatedCourseDto = createdCourse;
        Assert.NotNull(createdCourse);

        Assert.False(createdCourse.CourseId.Equals(Guid.Empty));
        Assert.False(createdCourse.ContentId.Equals(Guid.Empty));
        Assert.Equal(ValidOrgId, createdCourse.OrganizationId);
        Assert.Equal(ValidContentCode, createdCourse.ContentCode);
        Assert.Equal(ValidCourseName, createdCourse.Title);
        Assert.Null(createdCourse.Description);
        Assert.True(createdCourse.BriefDescription.IsNullOrEmpty());
        Assert.Empty(createdCourse.LanguageIds);
        Assert.Equal(1, createdCourse.StatusId);
        Assert.IsType<DateTime>(createdCourse.Created);
        Assert.NotNull(createdCourse.CreatedBy);
        Assert.True(createdCourse.CreatedBy.Equals(GlobalTestSetup.NewAuditableEntityCreatedByUserId.ToString()));
        Assert.Null(createdCourse.LastModified);
        Assert.Null(createdCourse.LastModifiedBy);
    }

    [Then("a LearningContent document should exist in Cosmos for the created course")]
    public async Task ThenLearningContentDocumentShouldExistInCosmos()
    {
        Assert.NotNull(courseContext.CreatedCourseDto);

        var cosmosClientWrapper = GlobalTestSetup.Factory!.Services.GetRequiredService<ICosmosClientWrapper>();

        var learningContent = await cosmosClientWrapper.QueryFirstOrDefaultAsync<Domain.Course.LearningContent.LearningContent>(
            q => q.Where(lc => lc.CourseId == courseContext.CreatedCourseDto.CourseId),
            CancellationToken.None);

        Assert.NotNull(learningContent);
        Assert.Equal(courseContext.CreatedCourseDto.CourseId, learningContent.CourseId);
        Assert.NotEmpty(learningContent.Sections);
        Assert.Equal("Section 1", learningContent.Sections.First().Name.En);
    }

    [When("the user does not provide a course name")]
    public async Task WhenUserProvidesNoCourseName()
    {
        CreateCourseDto newCourse = new()
        {
            OrganizationId = ValidOrgId,
            CourseName = "",
            ContentCode = ValidContentCode
        };

        var request = HttpExtensions.CreateRequestBody(newCourse);

        httpResponseContext.Response = await _client.PostAsync(EndpointUri, request);
    }

    [When("the user does not provide a content code")]
    public async Task WhenUserProvidesNoContentCode()
    {
        CreateCourseDto newCourse = new()
        {
            OrganizationId = ValidOrgId,
            CourseName = ValidCourseName,
            ContentCode = ""
        };

        var request = HttpExtensions.CreateRequestBody(newCourse);

        httpResponseContext.Response = await _client.PostAsync(EndpointUri, request);
    }

    [When("the user provides a course name that exceeds the allowed limit")]
    public async Task WhenUserProvidesCourseNameExceedingAllowedCharacterLimit()
    {
        string longCourseName = new('a', 501);
        CreateCourseDto newCourse = new()
        {
            OrganizationId = ValidOrgId,
            CourseName = longCourseName,
            ContentCode = ValidContentCode
        };

        var request = HttpExtensions.CreateRequestBody(newCourse);

        httpResponseContext.Response = await _client.PostAsync(EndpointUri, request);
    }

    [When("the user provides a content code that exceeds the allowed limit")]
    public async Task WhenUserProvidesContentCodeExceedingAllowedCharacterLimit()
    {
        string longContentCode = new('a', 101);
        CreateCourseDto newCourse = new()
        {
            OrganizationId = ValidOrgId,
            CourseName = ValidCourseName,
            ContentCode = longContentCode
        };

        var request = HttpExtensions.CreateRequestBody(newCourse);

        httpResponseContext.Response = await _client.PostAsync(EndpointUri, request);
    }

    [When("the user provides a content code with special characters")]
    public async Task WhenUserProvidesContentCodeWithSpecialCharacters()
    {
        CreateCourseDto newCourse = new()
        {
            OrganizationId = ValidOrgId,
            CourseName = ValidCourseName,
            ContentCode = "INVALID.COURSE!CODE"
        };

        var request = HttpExtensions.CreateRequestBody(newCourse);

        httpResponseContext.Response = await _client.PostAsync(EndpointUri, request);
    }

    [When("the user provides a content code starting with special characters")]
    public async Task WhenUserProvidesContentCodeStartingWithSpecialCharacters()
    {
        CreateCourseDto newCourse = new()
        {
            OrganizationId = ValidOrgId,
            CourseName = ValidCourseName,
            // Dash is used as the first character to isolate the specific validation warning being tested.
            ContentCode = "-INVALID-COURSE-CODE"
        };

        var request = HttpExtensions.CreateRequestBody(newCourse);

        httpResponseContext.Response = await _client.PostAsync(EndpointUri, request);
    }

    [Given("an existing course")]
    public static async Task GivenExistingCourse()
    {
        var context = GlobalTestSetup.GetAuditableEntityCompatibleContext();

        var courseContentType = await context.ContentType.SingleOrDefaultAsync(ct => ct.ContentTypeId == 2);
        Assert.NotNull(courseContentType);
        Assert.Equal("Course", courseContentType.ContentTypeDescription);

        var draftStatus = await context.Statuses.SingleOrDefaultAsync(s => s.StatusId == 1);
        Assert.NotNull(draftStatus);
        Assert.Equal("Draft", draftStatus.Name);

        Domain.Content.Content content = new()
        {
            ContentId = Guid.NewGuid(),
            ContentTypeId = courseContentType.ContentTypeId,
            ContentType = courseContentType
        };

        Domain.Course.Course existingCourse = new()
        {
            CourseId = Guid.NewGuid(),
            ContentId = content.ContentId,
            OrganizationId = ValidOrgId,
            ContentCode = ExistingContentCode,
            Title = "Existing Course",
            StatusId = draftStatus.StatusId,
            Created = DateTime.Now,
            CreatedBy = GlobalTestSetup.NewAuditableEntityCreatedByUserId.ToString()
        };

        // Add existing course data
        await context.Content.AddAsync(content);
        await context.Courses.AddAsync(existingCourse);
        await context.SaveChangesAsync();
    }

    [When("the user provides a content code that already exists")]
    public async Task WhenUserProvidesExistingContentCode()
    {
        CreateCourseDto newCourse = new()
        {
            OrganizationId = ValidOrgId,
            CourseName = ValidCourseName,
            ContentCode = "C101"
        };

        var request = HttpExtensions.CreateRequestBody(newCourse);

        httpResponseContext.Response = await _client.PostAsync(EndpointUri, request);
    }   

    [When("the user does not have the required permissions")]
    public async Task WhenUserMissingRequiredPermission()
    {
        CreateCourseDto newCourse = new()
        {
            // Provide an Organization ID that doesn't match the OrgId of the user used in the token
            OrganizationId = TestOrgIds.Site1Org,
            CourseName = ValidCourseName,
            ContentCode = ExistingContentCode
        };

        var request = HttpExtensions.CreateRequestBody(newCourse);

        _client.SetAuthHeaderWithOrg(TestOrgIds.DefaultOrg, TestOrgIds.DefaultOrg);
        httpResponseContext.Response = await _client.PostAsync(EndpointUri, request);
    }

    [When("the user is not authenticated")]
    public async Task WhenUserNotAuthenticated()
    {
        CreateCourseDto newCourse = new()
        {
            OrganizationId = ValidOrgId,
            CourseName = ValidCourseName,
            ContentCode = ExistingContentCode
        };

        var request = HttpExtensions.CreateRequestBody(newCourse);

        _client.DefaultRequestHeaders.Authorization = null;
        httpResponseContext.Response = await _client.PostAsync(EndpointUri, request);
    }
}