using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Enums;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course.Enums;


namespace Relias.ContentLibraryService.App.Tests.Features.Course.Commands.FInalExam;

public class UpdateFinalExamQuestionCommandHandlerTests
{
    private readonly Mock<IFinalExamService> _serviceMock = new();
    private readonly UpdateFinalExamQuestionCommand.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private static readonly Guid validCourseId = Guid.NewGuid();
    private static readonly Guid validExamId = Guid.NewGuid();
    private static readonly Guid validQuestionId = Guid.NewGuid();
    private static readonly Guid validOptionId1 = Guid.NewGuid();
   

    public UpdateFinalExamQuestionCommandHandlerTests()
    {
        // FIX: instantiate handler with mock
        _handler = new UpdateFinalExamQuestionCommand.Handler(_serviceMock.Object);
    }
    private static readonly FinalExamQuestionPatchDto validUpdates = new()
    {
        QuestionUpdate = new FinalExamUpdateQuestionDto
        {
            Action = PatchAction.Add,
            QuestionText = new LocalizedStringDto { En = "Updated Question" },
            QuestionType = QuestionType.SingleSelect
        },
        QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
{
            new FinalExamQuestionUpdateOptionDto
            {
                Action =  PatchAction.Add,
                OptionText = new LocalizedStringDto { En = "Updated Option 1" },
                ResponseFeedback = new LocalizedStringDto { En = "Updated Response Feedback 1" },
                IsCorrect = true
            },
            new FinalExamQuestionUpdateOptionDto
            {
                Action =  PatchAction.Add,
                OptionText = new LocalizedStringDto { En = "Updated Option 2" },
                ResponseFeedback = new LocalizedStringDto { En = "Updated Response Feedback 2" },
                IsCorrect = false
            },
            new FinalExamQuestionUpdateOptionDto
            {
                Action =  PatchAction.Add,
                OptionText = new LocalizedStringDto { En = "Updated Option 3" },
                ResponseFeedback = new LocalizedStringDto { En = "Updated Response Feedback 3" },
                IsCorrect = false
            }
        }
    };


    [Fact]
    public async Task Handle_ShouldReturnFinalExamDto_WhenUpdateIsSuccessful()
    {
        // Arrange
        var contract = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = validCourseId,
            ExamId = validExamId,
            QuestionId = validQuestionId,
            Updates = validUpdates
        };

        var expectedExam = new FinalExamDto
        {
            Id = validExamId,
            FinalExamQuestions = new List<FinalExamQuestionDto>
        {
            new FinalExamQuestionDto
            {
                QuestionId = validQuestionId,
                QuestionText = new LocalizedStringDto { En = "Updated Question" },
                QuestionType = QuestionType.SingleSelect,
                QuestionOptions = new List<FinalExamQuestionOptionsDto>
                {
                    new FinalExamQuestionOptionsDto { OptionId = validOptionId1, OptionText =  new LocalizedStringDto { En = "Yes" }, IsCorrect = true }
                }
            }
        }
        };

        _serviceMock
            .Setup(s => s.UpdateFinalExamQuestionAsync(contract.CourseId, contract.ExamId, contract.QuestionId, contract.Updates, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedExam);

        // Act
        var result = await _handler.Handle(contract, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedExam.Id, result.Id);

        Assert.NotNull(result.FinalExamQuestions); // Prevents potential null dereference
        Assert.Single(result.FinalExamQuestions);

        _serviceMock.Verify(
            s => s.UpdateFinalExamQuestionAsync(contract.CourseId, contract.ExamId, contract.QuestionId, contract.Updates, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenServiceThrowsException()
    {
        // Arrange
        var contract = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = validCourseId,
            ExamId = validExamId,
            QuestionId = validQuestionId,
            Updates = validUpdates
        };

        _serviceMock
            .Setup(s => s.UpdateFinalExamQuestionAsync(contract.CourseId, contract.ExamId, contract.QuestionId, contract.Updates, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid update"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(contract, CancellationToken.None));
        Assert.Equal("Invalid update", ex.Message);

        _serviceMock.Verify(
            s => s.UpdateFinalExamQuestionAsync(contract.CourseId, contract.ExamId, contract.QuestionId, contract.Updates, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenRequiredFieldsAreMissing()
    {
        // Arrange
        var contract = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.Empty,
            ExamId = Guid.Empty,
            QuestionId = Guid.Empty,
            Updates = new FinalExamQuestionPatchDto()
        };

        _serviceMock
            .Setup(s => s.UpdateFinalExamQuestionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<FinalExamQuestionPatchDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Required fields missing"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(contract, CancellationToken.None));
    }



    


}