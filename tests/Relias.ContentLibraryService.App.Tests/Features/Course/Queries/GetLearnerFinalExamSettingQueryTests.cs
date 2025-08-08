using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.App.Features.Course.Queries.Learner;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Queries;
public class GetLearnerFinalExamSettingQueryTests
{
    private readonly Mock<IFinalExamService> _serviceMock;

    private readonly GetLearnerFinalExamSettingQuery.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetLearnerFinalExamSettingQueryTests()
    { 
        _serviceMock = new Mock<IFinalExamService>();
        _handler = new GetLearnerFinalExamSettingQuery.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenLearnerFinalExamExists_ReturnsFinalExamSettingsDto()
    {
        var courseId = Guid.NewGuid();
        var duration = new Duration
        {
            Hours = 1,
            Minutes = 30
        };

        var finalExamDto = new FinalExamSettingDto
        {
            MinimumPercentageToPass = 70,
            QuestionsDisplayedPerExam = 10,
            Duration = duration,
        };

        _serviceMock
            .Setup(service => service.GetFinalExamSettingsAsync(courseId, _cancellationToken))
                .ReturnsAsync(finalExamDto);

        var query = new GetLearnerFinalExamSettingQuery.Contract { CourseId = courseId };

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(finalExamDto.MinimumPercentageToPass, result.MinimumPercentageToPass);
        Assert.Equal(finalExamDto.QuestionsDisplayedPerExam, result.QuestionsDisplayedPerExam);
        Assert.Equal(finalExamDto.Duration.Hours, result.Duration?.Hours);
        Assert.Equal(finalExamDto.Duration.Minutes, result.Duration?.Minutes);

        _serviceMock.Verify(repo => repo.GetFinalExamSettingsAsync(courseId, _cancellationToken), Times.Once);
    }

}