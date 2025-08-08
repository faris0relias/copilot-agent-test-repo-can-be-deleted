using Microsoft.Azure.Cosmos;
using Moq;
using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Relias.ContentLibraryService.Infra.Cosmos;
using Relias.ContentLibraryService.Infra.Repositories;

namespace Relias.ContentLibraryService.Infra.Tests.Repositories;

public class LearningContentRepositoryTests
{
    private readonly Mock<ICosmosClientWrapper> _mockClientWrapper = new();
    private readonly LearningContentRepository _repository;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public LearningContentRepositoryTests()
    {
        _repository = new LearningContentRepository(_mockClientWrapper.Object);
    }

    [Fact]
    public async Task GetByCourseIdAsync_WhenLearningContentExists_ReturnsLearningContent()
    {
        var courseId = Guid.NewGuid();
        var expected = new LearningContent
        {
            Id = Guid.NewGuid(),
            CourseId = courseId
        };

        _mockClientWrapper
            .Setup(c => c.QueryFirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<LearningContent>, IQueryable<LearningContent>>>(),
                _cancellationToken))
            .ReturnsAsync(expected);

        var result = await _repository.GetByCourseIdAsync(courseId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expected.CourseId, result!.CourseId);
        _mockClientWrapper.Verify(c => c.QueryFirstOrDefaultAsync(It.IsAny<Func<IQueryable<LearningContent>, IQueryable<LearningContent>>>(), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetByCourseIdAsync_WhenNoLearningContentExists_ReturnsNull()
    {
        var courseId = Guid.NewGuid();

        _mockClientWrapper
            .Setup(c => c.QueryFirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<LearningContent>, IQueryable<LearningContent>>>(),
                _cancellationToken))
            .ReturnsAsync((LearningContent?)null);

        var result = await _repository.GetByCourseIdAsync(courseId, _cancellationToken);

        Assert.Null(result);
        _mockClientWrapper.Verify(c => c.QueryFirstOrDefaultAsync(It.IsAny<Func<IQueryable<LearningContent>, IQueryable<LearningContent>>>(), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsLearningContent()
    {
        var learningContent = new LearningContent
        {
            Id = Guid.NewGuid(),
            CourseId = Guid.NewGuid()
        };

        _mockClientWrapper
            .Setup(c => c.CreateItemAsync(learningContent, _cancellationToken))
            .ReturnsAsync(learningContent);

        var result = await _repository.CreateAsync(learningContent, _cancellationToken);

        Assert.Equal(learningContent.Id, result.Id);
        Assert.Equal(learningContent.CourseId, result.CourseId);
        _mockClientWrapper.Verify(c => c.CreateItemAsync(learningContent, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenLearningContentExists_DeletesSuccessfully()
    {
        var learningContent = new LearningContent
        {
            Id = Guid.NewGuid(),
            CourseId = Guid.NewGuid()
        };

        var partitionKey = new PartitionKey(learningContent.CourseId.ToString());

        _mockClientWrapper
     .Setup(c => c.DeleteItemAsync<LearningContent>(
         It.IsAny<string>(),
         It.IsAny<PartitionKey>(),
         It.IsAny<CancellationToken>()))
     .ReturnsAsync(new LearningContent());

        await _repository.DeleteAsync(learningContent, _cancellationToken);

        _mockClientWrapper.Verify(
            c => c.DeleteItemAsync<LearningContent>(learningContent.Id.ToString(), partitionKey, _cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenLearningContentIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.DeleteAsync(null!, _cancellationToken));
    }
}
