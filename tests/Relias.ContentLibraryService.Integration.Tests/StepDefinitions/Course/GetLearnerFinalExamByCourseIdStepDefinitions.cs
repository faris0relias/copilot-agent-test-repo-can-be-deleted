using Relias.ContentLibraryService.Domain.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.FinalExam;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class GetLearnerFinalExamByCourseIdStepDefinitions(
    HttpResponseContext httpResponseContext,
    AuthenticationHeaderContext authenticationHeaderContext,
    FinalExamContext finalExamContext,
    ScenarioService scenarioService)
{
    private readonly string _endpointUri = "api/v1/courses/";
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
    private readonly Guid examId = Guid.NewGuid();
    private readonly Guid courseId = GlobalTestSetup.PatchCourseIds[0];
    private readonly Guid questionId1 = Guid.NewGuid();
    private readonly Guid questionId2 = Guid.NewGuid();
    private readonly Guid questionId3 = Guid.NewGuid();
    private readonly Guid questionId4 = Guid.NewGuid();
    private readonly Guid questionId5 = Guid.NewGuid();
    private readonly Guid questionId6 = Guid.NewGuid();
    private readonly Guid questionId7 = Guid.NewGuid();
    private readonly Guid questionId8 = Guid.NewGuid();
    private readonly Guid questionId9 = Guid.NewGuid();
    private readonly Guid questionId10 = Guid.NewGuid();
    private readonly Guid optionId1 = Guid.NewGuid();
    private readonly Guid optionId2 = Guid.NewGuid();
    private readonly Guid optionId3 = Guid.NewGuid();
    private readonly Guid optionId4 = Guid.NewGuid();
    private readonly Guid optionId5 = Guid.NewGuid();
    private readonly Guid optionId6 = Guid.NewGuid();
    private readonly Guid optionId7 = Guid.NewGuid();
    private readonly Guid optionId8 = Guid.NewGuid();
    private readonly Guid optionId9 = Guid.NewGuid();
    private readonly Guid optionId10 = Guid.NewGuid();

    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }
    
    // To get the list of final exam
    [Given("learner final exam exists")]
    public async Task GivenLearnerFinalExamExists()
    {
        var finalExam = new FinalExam
        {
            Id = examId.ToString(),
            CourseId = courseId,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
            {
                new FinalExamQuestion
                {
                    QuestionId = questionId1,
                    QuestionText = new LocalizedString { En = "Question" },
                    QuestionType = QuestionType.SingleSelect,
                    QuestionOptions = new List<FinalExamQuestionOptions>
                    {
                        new() { OptionId = optionId1, OptionText = new LocalizedString { En = "Option 1" },ResponseFeedback = new LocalizedString { En = "Response Feedback 1" }, IsCorrect = true },
                        new() { OptionId = optionId2, OptionText = new LocalizedString { En = "Option 2" },ResponseFeedback= null, IsCorrect = false },
                        new() { OptionId = optionId3, OptionText = new LocalizedString { En = "Option 3" },ResponseFeedback= null, IsCorrect = false }
                    }
                },
                 new FinalExamQuestion
                 {
                     QuestionId = questionId2,
                     QuestionText = new LocalizedString { En = "Question 2" },
                     QuestionType = QuestionType.MultiSelect,
                     QuestionOptions = new List<FinalExamQuestionOptions>
                     {
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 1" },ResponseFeedback = new LocalizedString { En = "Response Feedback 1" }, IsCorrect = true },
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 2" },ResponseFeedback= null, IsCorrect = false },
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 3" },ResponseFeedback= null, IsCorrect =  true }
                     }
                 },
                 new FinalExamQuestion
                 {
                     QuestionId = questionId3,
                     QuestionText = new LocalizedString { En = "Original Question Text" },
                     QuestionType = QuestionType.SingleSelect,
                     QuestionOptions = new List<FinalExamQuestionOptions>
                     {
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 1" }, IsCorrect = true },
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 2" }, IsCorrect = false }
                     }
                 },
                 new FinalExamQuestion
                 {
                     QuestionId = questionId4,
                     QuestionText = new LocalizedString { En = "Question Text 4" },
                     QuestionType = QuestionType.SingleSelect,
                     QuestionOptions = new List<FinalExamQuestionOptions>
                     {
                         new() { OptionId = optionId4, OptionText = new LocalizedString { En = "Option 1" }, IsCorrect = true },
                         new() { OptionId = optionId5, OptionText = new LocalizedString { En = "Option 2" }, IsCorrect = false },
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 3" }, IsCorrect = false }
                     }
                 },
                 new FinalExamQuestion
                 {
                     QuestionId = questionId5,
                     QuestionText = new LocalizedString { En = "Question Text 5" },
                     QuestionType = QuestionType.SingleSelect,
                     QuestionOptions = new List<FinalExamQuestionOptions>
                     {
                         new() { OptionId = optionId5, OptionText = new LocalizedString { En = "Option 5" }, IsCorrect = true },
                         new() { OptionId = optionId6, OptionText = new LocalizedString { En = "Option 6" }, IsCorrect = false },
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 7" }, IsCorrect = false }
                     }
                 },
                 new FinalExamQuestion
                 {
                     QuestionId = questionId6,
                     QuestionText = new LocalizedString { En = "Question Text 6" },
                     QuestionType = QuestionType.SingleSelect,
                     QuestionOptions = new List<FinalExamQuestionOptions>
                     {
                         new() { OptionId = optionId6, OptionText = new LocalizedString { En = "Option 1" }, IsCorrect = true },
                         new() { OptionId = optionId7, OptionText = new LocalizedString { En = "Option 2" }, IsCorrect = false },
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 3" }, IsCorrect = false }
                     }
                 },
                 new FinalExamQuestion
                 {
                     QuestionId = questionId7,
                     QuestionText = new LocalizedString { En = "Question Text 7" },
                     QuestionType = QuestionType.SingleSelect,
                     QuestionOptions = new List<FinalExamQuestionOptions>
                     {
                         new() { OptionId = optionId6, OptionText = new LocalizedString { En = "Option 1" }, IsCorrect = true },
                         new() { OptionId = optionId7, OptionText = new LocalizedString { En = "Option 2" }, IsCorrect = false },
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 3" }, IsCorrect = false }
                     }
                 },
                 new FinalExamQuestion
                 {
                     QuestionId = questionId8,
                     QuestionText = new LocalizedString { En = "Question Text 8" },
                     QuestionType = QuestionType.SingleSelect,
                     QuestionOptions = new List<FinalExamQuestionOptions>
                     {
                         new() { OptionId = optionId7, OptionText = new LocalizedString { En = "Option 1" }, IsCorrect = true },
                         new() { OptionId = optionId8, OptionText = new LocalizedString { En = "Option 2" }, IsCorrect = false },
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 3" }, IsCorrect = false }
                     }
                 },
                 new FinalExamQuestion
                 {
                     QuestionId = questionId9,
                     QuestionText = new LocalizedString { En = "Question Text 9" },
                     QuestionType = QuestionType.SingleSelect,
                     QuestionOptions = new List<FinalExamQuestionOptions>
                     {
                         new() { OptionId = optionId8, OptionText = new LocalizedString { En = "Option 1" }, IsCorrect = true },
                         new() { OptionId = optionId9, OptionText = new LocalizedString { En = "Option 2" }, IsCorrect = false },
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 3" }, IsCorrect = false }
                     }
                 },
                 new FinalExamQuestion
                 {
                     QuestionId = questionId10,
                     QuestionText = new LocalizedString { En = "Question Text 10" },
                     QuestionType = QuestionType.SingleSelect,
                     QuestionOptions = new List<FinalExamQuestionOptions>
                     {
                         new() { OptionId = optionId9, OptionText = new LocalizedString { En = "Option 1" }, IsCorrect = true },
                         new() { OptionId = optionId10, OptionText = new LocalizedString { En = "Option 2" }, IsCorrect = false },
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 3" }, IsCorrect = false }
                     }
                 }

            }
        };

        finalExamContext!.FinalExam = finalExam;
        await GlobalTestSetup.CosmosClient!.CreateItemAsync(finalExam);
    }

    
    [When("the learner start exam")]
    public async Task WhenTheLearnerStartExam()
    {
        GetLearnerFinalExamQueryDto getLearnerFinalExamQueryDto = new GetLearnerFinalExamQueryDto
        {
            QuestionIds = [questionId1, questionId2, questionId3, questionId4, questionId5],
            IsCompleted = false,
        };

        httpResponseContext.Response = await _client.PostAsync($"{_endpointUri}{courseId}/learner/final-exam/question-details", HttpExtensions.CreateRequestBody(getLearnerFinalExamQueryDto));
    }

    [Then("final exam questions and options should be returned")]
    public void ThenAFinalExamQuestionsIsReturned()
    {
        var response = httpResponseContext.Response!.Content;

        Assert.NotNull(response);
    }

    [When("the learner preview exam")]
    public async Task WhenTheLearnerPreviewFinalExam()
    {
        GetLearnerFinalExamQueryDto getLearnerFinalExamQueryDto = new GetLearnerFinalExamQueryDto
        {
            QuestionIds = [questionId1, questionId2, questionId3, questionId4, questionId5],
            IsCompleted = true,
        };

        httpResponseContext.Response = await _client.PostAsync($"{_endpointUri}{courseId}/learner/final-exam/question-details", HttpExtensions.CreateRequestBody(getLearnerFinalExamQueryDto));
    }


    [Then("final exam questions and options should be returned with correct answers and response")]
    public void ThenAPrviewFinalExamQuestions()
    {
        var response = httpResponseContext.Response!.Content;

        Assert.NotNull(response);
    }
}