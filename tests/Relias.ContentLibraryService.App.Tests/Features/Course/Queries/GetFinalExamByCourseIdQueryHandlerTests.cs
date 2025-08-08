using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Queries;

public class GetFinalExamByCourseIdQueryHandlerTests
{
    private readonly Mock<IFinalExamService> _serviceMock = new();
    private readonly GetFinalExamByCourseIdQuery.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetFinalExamByCourseIdQueryHandlerTests()
    {
        _handler = new GetFinalExamByCourseIdQuery.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenFinalExamExists_ReturnsFinalExamDto()
    {
        var courseId = Guid.NewGuid();
        var lang = "en";
        var duration = new Duration
        {
            Hours = 1,
            Minutes = 30
        };

        var finalExamDto = new FinalExamDto
        {
            MinimumPercentageToPass = 70,
            QuestionsDisplayedPerExam = 10,
            Duration = duration,
        };

        _serviceMock
            .Setup(service => service.GetFinalExamByCourseIdAsync(courseId, lang, _cancellationToken))
                .ReturnsAsync(finalExamDto);

        var query = new GetFinalExamByCourseIdQuery.Contract { CourseId = courseId, lang=lang };

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(finalExamDto.MinimumPercentageToPass, result.MinimumPercentageToPass);
        Assert.Equal(finalExamDto.QuestionsDisplayedPerExam, result.QuestionsDisplayedPerExam);
        Assert.Equal(finalExamDto.Duration.Hours, result.Duration?.Hours);
        Assert.Equal(finalExamDto.Duration.Minutes, result.Duration?.Minutes);

        _serviceMock.Verify(repo => repo.GetFinalExamByCourseIdAsync(courseId, lang, _cancellationToken), Times.Once);
    }

}
