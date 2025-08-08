using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Commands;

public class DeleteCourseCommandHandlerTests
{
    private readonly Mock<ICourseService> _serviceMock = new();
    private readonly Mock<ILearningContentService> _learningContentServiceMock = new();
    private readonly DeleteCourseCommand.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public DeleteCourseCommandHandlerTests()
    {
        _handler = new DeleteCourseCommand.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidCourseIdProvided_DeletesCourse()
    {
        var courseId = Guid.NewGuid();
        var command = new DeleteCourseCommand.Contract(courseId);

        _serviceMock
            .Setup(cs => cs.DeleteAsync(courseId, _cancellationToken))
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, _cancellationToken);

        _serviceMock.Verify(cs => cs.DeleteAsync(courseId, _cancellationToken), Times.Once);
    }
}