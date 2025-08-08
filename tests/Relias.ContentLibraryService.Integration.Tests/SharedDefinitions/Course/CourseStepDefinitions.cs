using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.SharedDefinitions.Course;

[Binding]
public class CourseStepDefinitions(
    AuthenticationHeaderContext authenticationHeaderContext, 
    ScenarioService scenarioService,
    ScenarioContext scenarioContext)
{
    [BeforeScenarioBlock]
    public void InitializeScenarioClient()
    {
        scenarioService.ScenarioHttpClient = GlobalTestSetup.Factory!.CreateClient();
    }

    [Given("A user is authorized to access courses")]
    public void GivenAnAuthorizedUserWithAccessToCourses()
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

    [Given("the user is authorized to delete courses")]
    public void GivenTheUserIsAuthorizedToDeleteCourses()
    {
        //List<int> orgIds = [TestOrgIds.Site1Org, TestOrgIds.DeleteCourseOrg];
        var authHeaderValue = IntegrationTestAuthHelper.GenerateAuthHeaderValueFromClaims(
            [
                new(UserTokenKeys.Subject, "100"),
                new(UserTokenKeys.UserId, "100"),
                new(UserTokenKeys.ClientId, "platform-integration-tests"),
                new(UserTokenKeys.Permissions, "32"),
                new(UserTokenKeys.OrganizationId, "9"),
                new(UserTokenKeys.OrganizationIds, "1"),
                new(UserTokenKeys.OrganizationIds, "9")
            ]);

        authenticationHeaderContext.AuthenticationHeader = authHeaderValue;
    }

    [Given("the user is authorized to update courses")]
    public void GivenTheUserIsAuthorizedToUpdateCourses() 
    {        
        var authHeaderValue = IntegrationTestAuthHelper.GenerateAuthHeaderValueFromClaims(
            [
                new(UserTokenKeys.Subject, "100"),
                new(UserTokenKeys.UserId, "100"),
                new(UserTokenKeys.ClientId, "platform-integration-tests"),
                new(UserTokenKeys.Permissions, "32"),
                new(UserTokenKeys.OrganizationIds,"1"),
                new(UserTokenKeys.OrganizationIds, "10")
            ]);

        authenticationHeaderContext.AuthenticationHeader = authHeaderValue;
    }

    [Given("the lesson type is {string}")]
    public void GivenTheLessonTypeIs(string lessonType)
    {
        scenarioContext["LessonType"] = lessonType;
    }

    [Given("the format type is {string}")]
    public void GivenTheFormatTypeIs(string formatType)
    {
        scenarioContext["FormatType"] = formatType;
    }
}
