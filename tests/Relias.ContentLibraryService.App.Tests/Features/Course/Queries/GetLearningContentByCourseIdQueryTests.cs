using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Queries;

public class GetLearningContentByCourseIdQueryHandlerTests
{
    private readonly Mock<ILearningContentService> _serviceMock = new();
    private readonly GetLearningContentByCourseIdQuery.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetLearningContentByCourseIdQueryHandlerTests()
    {
        _handler = new GetLearningContentByCourseIdQuery.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenLearningContentExists_ReturnsLearningContentDto()
    {
        var courseId = Guid.NewGuid();

        var expectedDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = Guid.NewGuid(),
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects = []
                }
            ]
        };

        _serviceMock
            .Setup(service => service.GetByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(expectedDto);

        var query = new GetLearningContentByCourseIdQuery.Contract { CourseId = courseId };

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedDto.CourseId, result.CourseId);
        Assert.Single(result.Sections);
        Assert.Equal("Section 1", result.Sections[0].Name.En);

        _serviceMock.Verify(service => service.GetByCourseIdAsync(courseId, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLearningContentDoesNotExist_ReturnsNull()
    {
        var courseId = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.GetByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync((LearningContentDto?)null);

        var query = new GetLearningContentByCourseIdQuery.Contract { CourseId = courseId };

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.Null(result);
        _serviceMock.Verify(service => service.GetByCourseIdAsync(courseId, _cancellationToken), Times.Once);
    }
}
