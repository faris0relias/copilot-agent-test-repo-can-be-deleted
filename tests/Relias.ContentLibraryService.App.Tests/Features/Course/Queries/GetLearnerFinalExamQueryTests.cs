using Moq;
using Relias.ContentLibraryService.App.Features.Course.Queries.Learner;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Queries;
public class GetLearnerFinalExamQueryTests
{
    private readonly Mock<IFinalExamService> _serviceMock;

    private readonly GetLearnerFinalExamQuery.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetLearnerFinalExamQueryTests()
    { 
        _serviceMock = new Mock<IFinalExamService>();
        _handler = new GetLearnerFinalExamQuery.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Call_GetQuestionsByIdsAsync_When_QuestionIds_Are_Provided()
    {
        // Arrange
        var lang = "en";
        var courseId = Guid.NewGuid();
        var questionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var expectedResult = new { Questions = "Test" };
        var contract = new GetLearnerFinalExamQuery.Contract
        {
            CourseId = courseId,
            QuestionIds = questionIds,
            IsCompleted = true,
            lang = lang
        };
        _serviceMock
            .Setup(s => s.GetQuestionsByIdsAsync(courseId, questionIds, true, lang, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var query = new GetLearnerFinalExamQuery.Contract { CourseId = courseId, lang = lang, QuestionIds = questionIds, IsCompleted = true};

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult, result);
        _serviceMock.Verify(s => s.GetQuestionsByIdsAsync(courseId, questionIds, true, lang, It.IsAny<CancellationToken>()), Times.Once);  
    }



}