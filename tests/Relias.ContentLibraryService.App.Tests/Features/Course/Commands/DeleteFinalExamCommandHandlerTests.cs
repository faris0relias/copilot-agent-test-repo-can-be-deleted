using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Commands;

public class DeleteFinalExamCommandHandlerTests
{
    private readonly Mock<IFinalExamService> _serviceMock = new();
    private readonly DeleteFinalExamCommand.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public DeleteFinalExamCommandHandlerTests()
    {
        _handler = new DeleteFinalExamCommand.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidCourseIdProvided_ReturnsTrue()
    {
        _serviceMock
            .Setup(cs => cs.DeleteFinalExamAsync(It.IsAny<Guid>(), _cancellationToken));

        DeleteFinalExamCommand.Contract command = new(
            Guid.NewGuid()
            );

        await _handler.Handle(command, _cancellationToken);

        _serviceMock.Verify(cs => cs.DeleteFinalExamAsync(command.CourseId, _cancellationToken), Times.Once);
    }
}
