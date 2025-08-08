using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Commands;

public class PublishCourseCommandHandlerTests
{
    private readonly Mock<ICourseService> _courseServiceMock = new();
    private readonly PublishCourseCommand.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public PublishCourseCommandHandlerTests()
    {
        _handler = new PublishCourseCommand.Handler(_courseServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCalled_InvokesPublishAsyncWithCorrectParameters()
    {
        var courseId = Guid.NewGuid();
        var publishDto = new PublishCourseDto
        {
            StatusId = 3,
            PublishBy = Guid.NewGuid().ToString(),
            PublishDate = DateTime.UtcNow.AddDays(1)
        };

        var command = new PublishCourseCommand.Contract
        {
            CourseId = courseId,
            PublishCourse = publishDto
        };

        _courseServiceMock
            .Setup(cs => cs.PublishAsync(courseId, publishDto, _cancellationToken))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, _cancellationToken);

        Assert.Equal(MediatR.Unit.Value, result);
        _courseServiceMock.Verify(cs => cs.PublishAsync(courseId, publishDto, _cancellationToken), Times.Once);
    }
}
