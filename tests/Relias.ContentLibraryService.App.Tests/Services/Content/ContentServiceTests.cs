using AutoMapper;
using Moq;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.App.Interfaces.Content;
using Relias.ContentLibraryService.App.Services.Content;

namespace Relias.ContentLibraryService.App.Tests.Services.Content;

public class ContentServiceTests
{
    private readonly Mock<IContentRepository> _contentRepositoryMock = new();
    private readonly Mock<ICourseRepository> _courseRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly List<Domain.Content.Content> _mockContents;
    private readonly List<Domain.Course.Course> _mockCourses;
    private readonly Guid _mockContentId;
    private readonly ContentService _service;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public ContentServiceTests()
    {
        _mockContentId = Guid.NewGuid();
        _mockContents =
        [
            new()
            {
                ContentId = _mockContentId,
                ContentTypeId = 2
            }
        ];

        _mockCourses =
        [
            new()
            {
                CourseId = Guid.NewGuid(),
                ContentId = _mockContentId,
                Title = "Test Course",
                Created = DateTime.UtcNow,
                TimeToCompleteInMinutes = 10
            }
        ];

        _contentRepositoryMock
            .Setup(cr => cr.GetContentByContentIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockContents);

        _courseRepositoryMock
            .Setup(cr => cr.GetCoursesByContentIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockCourses);

        _service = new ContentService(_contentRepositoryMock.Object, _courseRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetContentByIdsAsync_WhenNoIdsPassedIn_ReturnsEmptyList()
    {
        var result = await _service.GetContentByIdsAsync([], _cancellationToken);
        
        Assert.Empty(result);
        _contentRepositoryMock
            .Verify(cr => cr.GetContentByContentIdsAsync(It.IsAny<IEnumerable<Guid>>(), _cancellationToken), Times.Never);
        _courseRepositoryMock
            .Verify(cr => cr.GetCoursesByContentIdsAsync(It.IsAny<IEnumerable<Guid>>(), _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task GetContentByIdsAsync_WhenNoContentMatches_ReturnsEmptyList()
    {
        var ids = new List<Guid> { Guid.NewGuid() };

        _contentRepositoryMock
            .Setup(cr => cr.GetContentByContentIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _service.GetContentByIdsAsync(ids, _cancellationToken);

        Assert.Empty(result);
        _contentRepositoryMock
            .Verify(cr => cr.GetContentByContentIdsAsync(ids, It.IsAny<CancellationToken>()), Times.Once);
        _courseRepositoryMock
            .Verify(cr => cr.GetCoursesByContentIdsAsync(It.IsAny<IEnumerable<Guid>>(), _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task GetContentByIdsAsync_WhenContentNotKnownType_ReturnsEmptyList()
    {
        var ids = new List<Guid> { Guid.NewGuid() };

        var mockContents = new List<Domain.Content.Content>
        {
            new()
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = 1000
            }
        };

        _contentRepositoryMock
            .Setup(cr => cr.GetContentByContentIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockContents);

        var result = await _service.GetContentByIdsAsync(ids, _cancellationToken);

        Assert.Empty(result);
        _contentRepositoryMock
            .Verify(cr => cr.GetContentByContentIdsAsync(ids, _cancellationToken), Times.Once);
        _courseRepositoryMock
            .Verify(cr => cr.GetCoursesByContentIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetContentByIdsAsync_WhenCourseMatchesExist_ReturnsListOfCourseContentDtos()
    {
        var ids = new List<Guid> { _mockContentId };

        var result = await _service.GetContentByIdsAsync(ids, _cancellationToken);

        Assert.NotEmpty(result);
        _contentRepositoryMock
            .Verify(cr => cr.GetContentByContentIdsAsync(ids, _cancellationToken), Times.Once);
        _courseRepositoryMock
            .Verify(cr => cr.GetCoursesByContentIdsAsync(ids, _cancellationToken), Times.Once);
    }
}
