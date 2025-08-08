using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Queries;

public class GetCourseByIdQueryHandlerTests
{
    private readonly Mock<ICourseService> _serviceMock = new();
    
    private readonly GetCourseByIdQuery.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetCourseByIdQueryHandlerTests()
    {
        _handler = new GetCourseByIdQuery.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCourseExists_ReturnsCourseDto()
    {
        var courseId = Guid.NewGuid();

        var expectedCourseDto = new CourseDto
        {
            CourseId = courseId,
            ContentId = Guid.NewGuid(),
            OrganizationId = 1,
            ContentCode = "C101",
            Title = "Test Course",
            BriefDescription = "Brief Test",
            StatusId = 1,
            Created = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid().ToString(),
        };

        _serviceMock
            .Setup(service => service.GetByIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(expectedCourseDto);

        var query = new GetCourseByIdQuery.Contract { CourseId = courseId };

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedCourseDto.CourseId, result.CourseId);
        Assert.Equal(expectedCourseDto.ContentId, result.ContentId);
        Assert.Equal(expectedCourseDto.OrganizationId, result.OrganizationId);
        Assert.Equal(expectedCourseDto.ContentCode, result.ContentCode);
        Assert.Equal(expectedCourseDto.Title, result.Title);
        Assert.Equal(expectedCourseDto.BriefDescription, result.BriefDescription);
        Assert.Equal(expectedCourseDto.StatusId, result.StatusId);
        Assert.Equal(expectedCourseDto.Created, result.Created);
        Assert.Equal(expectedCourseDto.CreatedBy, result.CreatedBy);

        _serviceMock.Verify(service => service.GetByIdAsync(courseId, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCourseDoesNotExist_ReturnsNull()
    {
        var courseId = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.GetByIdAsync(courseId, _cancellationToken))
        .ReturnsAsync((CourseDto?)null);

        var query = new GetCourseByIdQuery.Contract { CourseId = courseId };

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.Null(result);
        _serviceMock.Verify(service => service.GetByIdAsync(courseId, _cancellationToken), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenCourseExistsByID_ReturnsCourseDto()
    {
        var courseId = Guid.NewGuid();

        var expectedCourseDto = new CourseDto
        {
            CourseId = courseId,
            ContentId = Guid.NewGuid(),
            OrganizationId = 1,
            ContentCode = "C101",
            Title = "Test Course",
            BriefDescription = "Brief Test",
            StatusId = 1,
            Created = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid().ToString(),
        };

        _serviceMock
            .Setup(service => service.GetByIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(expectedCourseDto);

        var query = new GetCourseByIdQuery.Contract { CourseId = courseId };

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedCourseDto.CourseId, result.CourseId);
        Assert.Equal(expectedCourseDto.ContentId, result.ContentId);
        Assert.Equal(expectedCourseDto.OrganizationId, result.OrganizationId);
        Assert.Equal(expectedCourseDto.ContentCode, result.ContentCode);
        Assert.Equal(expectedCourseDto.Title, result.Title);
        Assert.Equal(expectedCourseDto.BriefDescription, result.BriefDescription);
        Assert.Equal(expectedCourseDto.StatusId, result.StatusId);
        Assert.Equal(expectedCourseDto.Created, result.Created);
        Assert.Equal(expectedCourseDto.CreatedBy, result.CreatedBy);

        _serviceMock.Verify(service => service.GetByIdAsync(courseId, _cancellationToken), Times.Once);
    }
}
