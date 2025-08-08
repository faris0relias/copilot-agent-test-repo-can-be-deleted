using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Enums;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.FinalExam;
using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using System.Net;
using System.Net.Http.Json;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course;

[Binding]
public class PatchFinalExamQuestionStepDefinitions(HttpResponseContext httpResponseContext,
    AuthenticationHeaderContext authenticationHeaderContext,
    FinalExamContext finalExamContext,
    ScenarioService scenarioService)
{
    [BeforeStep]
    public void SetDefaultAuthorizationHeader()
    {
        if (authenticationHeaderContext.AuthenticationHeader is not null)
        {
            _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
        }
    }
    private readonly string _endpointUri = "api/v1/courses";
    private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
    private readonly HttpResponseContext httpResponseContext = httpResponseContext;
    private readonly FinalExamContext finalExamContext = finalExamContext;
    private readonly Guid examId = Guid.NewGuid();
    private readonly Guid courseId = GlobalTestSetup.PatchCourseIds[0];
    private readonly Guid questionId = Guid.NewGuid();
    private readonly Guid questionId1 = Guid.NewGuid();
    private readonly Guid questionId2 = Guid.NewGuid();
    private readonly Guid questionId3 = Guid.NewGuid();
    private readonly Guid questionId4 = Guid.NewGuid();
    private readonly Guid optionId1 = Guid.NewGuid();
    private readonly Guid optionId2 = Guid.NewGuid();
    private readonly Guid optionId3 = Guid.NewGuid();
    private readonly Guid optionId4 = Guid.NewGuid();
    private readonly Guid optionId5 = Guid.NewGuid();

    private string GetRequestUri(Guid questionId) =>
        $"{_endpointUri}/{courseId}/final-exam/{examId}/question/{questionId}";


    [Given("Final Exam exists")]
    public async Task GivenFinalExamExists()
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
                     QuestionText = new LocalizedString { En = "Question Text 2" },
                     QuestionType = QuestionType.SingleSelect,
                     QuestionOptions = new List<FinalExamQuestionOptions>
                     {
                         new() { OptionId = optionId4, OptionText = new LocalizedString { En = "Option 1" }, IsCorrect = true },
                         new() { OptionId = optionId5, OptionText = new LocalizedString { En = "Option 2" }, IsCorrect = false },
                         new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "Option 3" }, IsCorrect = false }
                     }
                 }

            }
        };

        finalExamContext!.FinalExam = finalExam;
        await GlobalTestSetup.CosmosClient!.CreateItemAsync(finalExam);
    }

    [When("the user tries to add a new single-select question with options and one correct answer")]
    public async Task WhenUserAddsSingleSelectQuestionWithTwoOptions()
    {

        var requestBody = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Add,
                QuestionId = questionId,
                QuestionText = new LocalizedStringDto { En = "What is the capital of France?" },
                QuestionType = QuestionType.SingleSelect
            },
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new()
            {
                Action = PatchAction.Add,
                OptionId = Guid.NewGuid(),
                OptionText = new LocalizedStringDto { En = "Paris" },
                IsCorrect = true
            },
            new()
            {
                Action = PatchAction.Add,
                OptionId = Guid.NewGuid(),
                OptionText = new LocalizedStringDto { En = "Madrid" },
                IsCorrect = false
            }
        }
        };

        httpResponseContext.Response = await _client.PatchAsync(
            GetRequestUri(questionId),
            HttpExtensions.CreateRequestBody(requestBody));
    }

    [Then("the new single-select question and its options should be saved correctly")]
    public async Task ThenTheNewQuestionAndOptionsShouldBeSavedCorrectly()
    {
        var updated = await httpResponseContext.Response!.Content.ReadFromJsonAsync<FinalExamDto>();

        var question = updated!.FinalExamQuestions!.FirstOrDefault(q => q.QuestionText.En == "What is the capital of France?");

        Assert.NotNull(question);
        Assert.Equal("What is the capital of France?", question!.QuestionText.En);
        Assert.Equal(2, question?.QuestionOptions?.Count);
        Assert.Equal(1, question?.QuestionOptions?.Count(o => o.IsCorrect == true));
    }

    [When("the user tries to add a new multi-select question with options and more than one correct answer")]
    public async Task WhenUserAddsMultiSelectQuestionWithMultipleCorrectOptions()
    {

        var patchDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Add,
                QuestionType = QuestionType.MultiSelect,
                QuestionText = new LocalizedStringDto { En = "Which of the following are fruits?" }
            },
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new()
            {
                Action = PatchAction.Add,
                OptionId = Guid.NewGuid(),
                OptionText = new LocalizedStringDto { En = "Apple" },
                ResponseFeedback = new LocalizedStringDto { En = "It is a fruit." },
                IsCorrect = true
            },
            new()
            {
                Action = PatchAction.Add,
                OptionId = Guid.NewGuid(),
                OptionText = new LocalizedStringDto { En = "Banana" },
                ResponseFeedback = new LocalizedStringDto { En = "It is a fruit." },
                IsCorrect = true
            },
            new()
            {
                Action = PatchAction.Add,
                OptionId = Guid.NewGuid(),
                OptionText = new LocalizedStringDto { En = "Carrot" },
                IsCorrect = false
            }
        }
        };

        httpResponseContext.Response = await _client.PatchAsync(
            GetRequestUri(questionId),
            HttpExtensions.CreateRequestBody(patchDto));
    }

    [Then("the new multi-select question and its options should be saved correctly")]
    public async Task ThenMultiSelectQuestionAndOptionsShouldBeSaved()
    {
        var updated = await httpResponseContext.Response!.Content.ReadFromJsonAsync<FinalExamDto>();
        var question = updated!.FinalExamQuestions!.FirstOrDefault(q => q.QuestionText.En == "Which of the following are fruits?");

        Assert.NotNull(question);
        Assert.Equal("Which of the following are fruits?", question!.QuestionText.En);
        Assert.Equal(3, question?.QuestionOptions?.Count);
        Assert.Equal(2, question?.QuestionOptions?.Count(o => o.IsCorrect == true));
    }


    [When("the user patch with remove action and its QuestionId")]
    public async Task WhenIPatchWithRemoveActionAndItsQuestionId()
    {
        var patchDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                QuestionId = questionId2, // Use the stored questionId
                Action = PatchAction.Remove
            }
        };

        httpResponseContext.Response = await _client.PatchAsync(
            GetRequestUri(questionId),
            HttpExtensions.CreateRequestBody(patchDto));
    }

    [Then("the question should be deleted from the final exam")]
    public async Task ThenTheQuestionShouldBeDeletedFromTheFinalExam()
    {
        var updatedExam = await httpResponseContext.Response!.Content.ReadFromJsonAsync<FinalExamDto>();
        Assert.DoesNotContain(updatedExam!.FinalExamQuestions!, q => q.QuestionId == questionId2);
    }

    [When("the user add two correct options")]
    public async Task WhenIAddTwoCorrectOptions()
    {

        var dto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {

                Action = PatchAction.Add,
                QuestionType = QuestionType.SingleSelect,
                QuestionText = new LocalizedStringDto { En = "Choose the correct answer" }
            },
            QuestionOptionsUpdate = new()
        {
            new() { OptionId = Guid.NewGuid(), Action = PatchAction.Add, OptionText = new() { En = "Option 1" }, IsCorrect = true },
            new() { OptionId = Guid.NewGuid(), Action = PatchAction.Add, OptionText = new() { En = "Option 2" }, IsCorrect = true }
        }
        };

        httpResponseContext.Response = await _client.PatchAsync(GetRequestUri(questionId), HttpExtensions.CreateRequestBody(dto));
    }

    [When("the user submit options with none marked correct")]
    public async Task WhenISubmitOptionsWithNoCorrect()
    {

        var patchDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Add,
                QuestionType = QuestionType.MultiSelect,
                QuestionText = new LocalizedStringDto { En = "Select all that apply" }
            },
            QuestionOptionsUpdate = new()
        {
            new() { Action = PatchAction.Add, OptionId = Guid.NewGuid(), OptionText = new() { En = "Option A" }, IsCorrect = false },
            new() { Action = PatchAction.Add, OptionId = Guid.NewGuid(), OptionText = new() { En = "Option B" }, IsCorrect = false }
        }
        };

        httpResponseContext.Response = await _client.PatchAsync(GetRequestUri(questionId), HttpExtensions.CreateRequestBody(patchDto));
    }

    [When("the user tries to update the question with empty text")]
    public async Task WhenUserTriesEmptyUpdate()
    {
        var request = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Replace,
                QuestionId = questionId,
                QuestionText = new LocalizedStringDto { En = "" }
            }
        };

        httpResponseContext.Response = await _client.PatchAsync(GetRequestUri(questionId), HttpExtensions.CreateRequestBody(request));
    }
    [Then(@"the update should fail with ""(.*)""")]
    public Task ThenUpdateShouldFailWithMessage(string expected)
    {
        Assert.NotNull(httpResponseContext.Response);
        Assert.Equal(HttpStatusCode.BadRequest, httpResponseContext.Response!.StatusCode);
        return Task.CompletedTask;
    }
    [When("the user tries to add a question with an invalid question type")]
    public async Task WhenUserTriesToAddAQuestionWithInvalidQuestionType()
    {
        // Simulate invalid enum: cast an out-of-range int to the enum
        var invalidEnumValue = (QuestionType)999;

        var request = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Add,
                QuestionId = Guid.NewGuid(),
                QuestionType = invalidEnumValue,
                QuestionText = new LocalizedStringDto { En = "Invalid enum test" }
            }
        };

        httpResponseContext.Response = await _client.PatchAsync(
            GetRequestUri(Guid.NewGuid()),
            HttpExtensions.CreateRequestBody(request)
        );
    }

    [When("the user patch the question with more than 50 options")]
    public async Task WhenIPatchQuestionWithTooManyOptions()
    {


        var options = Enumerable.Range(1, 52).Select(i => new FinalExamQuestionUpdateOptionDto
        {
            Action = PatchAction.Add,
            OptionId = Guid.NewGuid(),
            OptionText = new LocalizedStringDto { En = $"Option {i}" },
            IsCorrect = i == 1
        }).ToList();

        var patchDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                QuestionId = questionId,
                Action = PatchAction.Add,
                QuestionType = QuestionType.SingleSelect,
                QuestionText = new LocalizedStringDto { En = "Overflow of options" }
            },
            QuestionOptionsUpdate = options
        };

        httpResponseContext.Response = await _client.PatchAsync(GetRequestUri(questionId), HttpExtensions.CreateRequestBody(patchDto));
    }

    

    [When("the user try to remove correct option")]
    public async Task WhenITryToRemoveOnlyCorrectOption()
    {

        var patchDto = new FinalExamQuestionPatchDto
        {
            QuestionOptionsUpdate =
            new()
            {
                 new FinalExamQuestionUpdateOptionDto
                 {
                       Action = PatchAction.Remove,
                       OptionId = optionId1,
                       OptionText = new  LocalizedStringDto { En = "Option 1" },
                       ResponseFeedback = new LocalizedStringDto { En = "Response Feedback 1" },
                       IsCorrect = true
                 }
            }
        };

        httpResponseContext.Response = await _client.PatchAsync(GetRequestUri(questionId1), HttpExtensions.CreateRequestBody(patchDto));
    }

    
   


}