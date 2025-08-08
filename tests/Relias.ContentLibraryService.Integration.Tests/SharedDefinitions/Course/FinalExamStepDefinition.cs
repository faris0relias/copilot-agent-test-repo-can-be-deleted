using Microsoft.Azure.Cosmos;
using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.Domain.Course.FinalExam;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.SharedDefinitions.Course;

[Binding]
public class FinalExamStepDefinition(
    AuthenticationHeaderContext authenticationHeaderContext,
    ScenarioService scenarioService,
    FinalExamContext finalExamContext,
    HttpResponseContext httpResponseContext)
{
    private const string _endpointUri = "api/v1/courses";
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient!;

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }

    [Given("a course exists for the organization")]
    public async Task GivenCourseExists()
    {
        CreateCourseDto newCourse = new()
        {
            OrganizationId = TestOrgIds.DefaultOrg,
            CourseName = Guid.NewGuid().ToString(),
            ContentCode = Guid.NewGuid().ToString()
        };

        var request = HttpExtensions.CreateRequestBody(newCourse);

        httpResponseContext.Response = await _client.PostAsync(_endpointUri, request);
        var courseDto = await httpResponseContext.Response.Content.As<CourseDto>();
        finalExamContext.CourseId = courseDto!.CourseId;
    }

    [Given("the final exam exists")]
    public async Task GivenFinalExamExists()
    {
        var finalExamData = new FinalExam
        {
            Id = Guid.NewGuid().ToString(), CourseId = finalExamContext.CourseId, Created = DateTime.UtcNow
           
        };
        finalExamContext.FinalExam =
            await GlobalTestSetup.CosmosClient!.CreateItemAsync(finalExamData, It.IsAny<CancellationToken>());
        Assert.NotNull(finalExamContext.FinalExam);
    }

    [Given("the final exam does not exist")]
    public async Task GivenFinalExamDoesNotExists()
    {
        if (finalExamContext.FinalExam is not null)
        {
            await GlobalTestSetup.CosmosClient!.DeleteItemAsync<FinalExam>(
                finalExamContext.FinalExam.Id,
                new PartitionKey(finalExamContext.CourseId.ToString()),
                It.IsAny<CancellationToken>());
        }
    }
}