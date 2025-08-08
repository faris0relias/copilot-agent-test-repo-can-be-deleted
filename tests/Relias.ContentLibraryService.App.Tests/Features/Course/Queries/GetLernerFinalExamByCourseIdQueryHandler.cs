using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Queries;

public class GetLernerFinalExamByCourseIdQueryHandlerTests
{
    private readonly Mock<IFinalExamService> _serviceMock = new();
    private readonly GetLearnerFinalExamSettingWithAnswerIdByCourseIdQuery.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetLernerFinalExamByCourseIdQueryHandlerTests()
    {
        _handler = new GetLearnerFinalExamSettingWithAnswerIdByCourseIdQuery.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenFinalExamExists_ReturnsFinalExamSetupDto()
    {
        var courseId = Guid.NewGuid();
        var duration = new Duration
        {
            Hours = 1,
            Minutes = 30
        };
        var questionAnswerPairDto = new List<QuestionAnswerPairDto>
        {
            new QuestionAnswerPairDto
            {
                QuestionId = Guid.NewGuid(),
                CorrectOptionIds = new List<Guid>(){ Guid.NewGuid() }
            }
        };
        var finalExamSetupDto = new FinalExamSettingWithAnswerDto
        {
            MinimumPercentageToPass = 70,
            QuestionsDisplayedPerExam = 10,
            Duration = duration,
            QuestionAnswerPairs = questionAnswerPairDto
        };

        _serviceMock
            .Setup(service => service.GetLearnerFinalExamQuestionAnswerIdsAsync(courseId, _cancellationToken))
                .ReturnsAsync(finalExamSetupDto);

        var query = new GetLearnerFinalExamSettingWithAnswerIdByCourseIdQuery.Contract { CourseId = courseId};

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(finalExamSetupDto.MinimumPercentageToPass, result.MinimumPercentageToPass);
        Assert.Equal(finalExamSetupDto.QuestionsDisplayedPerExam, result.QuestionsDisplayedPerExam);
        Assert.Equal(finalExamSetupDto.Duration.Hours, result.Duration?.Hours);
        Assert.Equal(finalExamSetupDto.Duration.Minutes, result.Duration?.Minutes);
        Assert.NotNull(result.QuestionAnswerPairs);
        Assert.NotEmpty(result.QuestionAnswerPairs);

        var question = result.QuestionAnswerPairs[0];
        Assert.NotEmpty(question.CorrectOptionIds);
        Assert.NotNull(question.CorrectOptionIds);

        _serviceMock.Verify(repo => repo.GetLearnerFinalExamQuestionAnswerIdsAsync(courseId, _cancellationToken), Times.Once);
    }

}
