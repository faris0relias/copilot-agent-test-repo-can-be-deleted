using Moq;
using Microsoft.Azure.Cosmos;
using Relias.ContentLibraryService.Common.Services;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Infra.Cosmos;
using Relias.ContentLibraryService.Infra.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.Infra.Tests.Repositories;

public class FinalExamRepositoryTests
{

    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICosmosClientWrapper> _cosmosClientWrapperMock;
    private readonly FinalExamRepository _repository;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public FinalExamRepositoryTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserServiceMock.Setup(s => s.UserId).Returns("1234-5678-9012-3456");
        _cosmosClientWrapperMock = new Mock<ICosmosClientWrapper>();
        _repository = new FinalExamRepository(_cosmosClientWrapperMock.Object);
    }

    [Fact]
    public async Task GetFinalExamAsync_ReturnsFinalExamIfNotEmpty()
    {
        var courseId = Guid.NewGuid();
        var expected = new FinalExam()
        {

            CourseId = courseId,
            Created = DateTime.UtcNow,
            CreatedBy = _currentUserServiceMock.Object.UserId

        };

        _cosmosClientWrapperMock
    .Setup(c => c.QueryFirstOrDefaultAsync(
        It.IsAny<Func<IQueryable<FinalExam>, IQueryable<FinalExam>>>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(expected);

        var result = await _repository.GetFinalExamByCourseIdAsync(courseId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(courseId, result.CourseId);
    }

    [Fact]
    public async Task GetLearnerFinalExamSettingAsync_ReturnsFinalExamIfNotEmpty()
    {
        var courseId = Guid.NewGuid();
        var expected = new FinalExam()
        {

            CourseId = courseId,
            Created = DateTime.UtcNow,
            CreatedBy = _currentUserServiceMock.Object.UserId

        };

        _cosmosClientWrapperMock
    .Setup(c => c.QueryFirstOrDefaultAsync(
        It.IsAny<Func<IQueryable<FinalExam>, IQueryable<FinalExam>>>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(expected);

        var result = await _repository.GetFinalExamSettingsAsync(courseId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(courseId, result.CourseId);
    }

    [Fact]
    public async Task GetFinalExamAsync_ReturnsNullIfNoFinalExamExists()
    {
        var courseId = Guid.NewGuid();

        _cosmosClientWrapperMock
            .Setup(c => c.QueryFirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<FinalExam>, IQueryable<FinalExam>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinalExam?)null);

        var result = await _repository.GetFinalExamByCourseIdAsync(courseId, _cancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLearnerFinalExamSettingAsync_ReturnsNullIfNoFinalExamExists()
    {
        var courseId = Guid.NewGuid();

        _cosmosClientWrapperMock
            .Setup(c => c.QueryFirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<FinalExam>, IQueryable<FinalExam>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinalExam?)null);

        var result = await _repository.GetFinalExamSettingsAsync(courseId, _cancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateFinalExamAsync_CreatesAndReturnsFinalExam()
    {
        var courseId = Guid.NewGuid();
        var userId = _currentUserServiceMock.Object.UserId;
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };

        _cosmosClientWrapperMock
            .Setup(c => c.CreateItemAsync(It.Is<FinalExam>(e => e.CourseId == courseId && e.CreatedBy == userId), _cancellationToken))
            .ReturnsAsync(finalExam);

        var result = await _repository.CreateFinalExamAsync(courseId, userId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(courseId, result.CourseId);
        Assert.Equal(userId, result.CreatedBy);
    }

    [Fact]
    public async Task UpdateFinalExamAsync_UpdatesAndReturnsFinalExam()
    {
        Guid courseId = Guid.Parse("b32788f5-d1a1-44a0-a0a3-6f1f9e5c6c44");
        var userId = _currentUserServiceMock.Object.UserId;

        var existingFinalExam = new FinalExam
        {
            Id = courseId.ToString(),
            CourseId = courseId,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };

        var updatedValues = new Dictionary<string, dynamic>
        {
            ["MinimumPercentageToPass"] = 75,
        };

        IList<string> propertiesToPatch = new List<string> { "MinimumPercentageToPass", "LastModified", "LastModifiedBy" };

        _cosmosClientWrapperMock
            .Setup(c => c.QueryFirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<FinalExam>, IQueryable<FinalExam>>>(),
                _cancellationToken))
            .ReturnsAsync(existingFinalExam);

        _cosmosClientWrapperMock
            .Setup(c => c.PatchItemAsync(
                existingFinalExam,
                courseId.ToString(),
                new PartitionKey(courseId.ToString()),
                propertiesToPatch,
                _cancellationToken
            ))
            .ReturnsAsync((FinalExam exam, string id, PartitionKey partitionKeyValue, IEnumerable<string> flds, CancellationToken ct) =>
            {
                exam.MinimumPercentageToPass = 75;
                return exam;
            });


        var result = await _repository.UpdateFinalExamAsync(courseId, userId, updatedValues, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(courseId, result.CourseId);
        Assert.Equal(75, result.MinimumPercentageToPass);
        _cosmosClientWrapperMock.Verify(c => c.PatchItemAsync<FinalExam>(
                                        existingFinalExam,
                                        courseId.ToString(),
                                        new PartitionKey(courseId.ToString()),
                                        propertiesToPatch,
                                        _cancellationToken), Times.Once);

    }

    [Fact]
    public async Task DeleteFinalExamAsync_DeletesFinalExamSuccessfully()
    {
        var courseId = Guid.NewGuid();
        var userId = _currentUserServiceMock.Object.UserId;

        var finalExam = new FinalExam
        {
            Id = Guid.NewGuid().ToString(),
            CourseId = courseId,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };

        var partitionKey = new PartitionKey(finalExam.CourseId.ToString());

        _cosmosClientWrapperMock
            .Setup(c => c.DeleteItemAsync<FinalExam>(finalExam.Id, partitionKey, _cancellationToken))
            .ReturnsAsync(finalExam);

        await _repository.DeleteFinalExamAsync(finalExam, _cancellationToken);

        _cosmosClientWrapperMock.Verify(c => c.DeleteItemAsync<FinalExam>(finalExam.Id, partitionKey, _cancellationToken), Times.Once);
    }
    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldCallCosmosClientWrapperAndReturnUpdatedEntity()
    {
        var courseId = Guid.NewGuid();
        var userId = _currentUserServiceMock.Object.UserId;

        
        var finalExam = new FinalExam
        {

            Id = Guid.NewGuid().ToString(),
            CourseId = courseId,
            Created = DateTime.UtcNow,
            CreatedBy = userId,
            FinalExamQuestions = new List<FinalExamQuestion>(),
            LastModified= DateTime.UtcNow,
            LastModifiedBy = userId,
        };
       

        _cosmosClientWrapperMock
            .Setup(c => c.UpdateItemAsync(finalExam, finalExam.Id,  _cancellationToken))
            .ReturnsAsync(finalExam);

        // Act
        var result = await _repository.UpdateFinalExamQuestionAsync(finalExam, It.IsAny<string>(), _cancellationToken);

        // Assert
        Assert.NotNull(result);
       
        _cosmosClientWrapperMock.Verify(c =>
            c.UpdateItemAsync(finalExam, finalExam.Id,  _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetLearnerFinalExamByCourseIdAsync_ReturnsFinalExamWithFilteredQuestions_WhenMatchingQuestionsExist()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var questionId1 = Guid.NewGuid();
        var questionId2 = Guid.NewGuid();
        var questionIds = new List<Guid> { questionId1, questionId2 };

        var originalQuestions = new List<FinalExamQuestion>
    {
        new FinalExamQuestion { QuestionId = questionId1, QuestionText = new LocalizedString { En="Question 1" } },
        new FinalExamQuestion { QuestionId = questionId2, QuestionText = new LocalizedString { En="Question 2" } },
        
    };

        var expected = new FinalExam
        {
            CourseId = courseId,
            FinalExamQuestions = originalQuestions,
            Created = DateTime.UtcNow,
            CreatedBy = _currentUserServiceMock.Object.UserId
        };

        _cosmosClientWrapperMock
            .Setup(c => c.QueryFirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<FinalExam>, IQueryable<FinalExam>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _repository.GetLearnerFinalExamByCourseIdAsync(courseId, questionIds, isCompleted: false, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(courseId, result.CourseId);
        Assert.NotNull(result.FinalExamQuestions);
        Assert.Equal(2, result.FinalExamQuestions.Count);
        Assert.All(result.FinalExamQuestions, q => Assert.Contains(q.QuestionId, questionIds));
    }
    [Fact]
    public async Task GetLearnerFinalExamByCourseIdAsync_ReturnsNull_WhenNoMatchingFinalExamFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var questionIds = new List<Guid> { Guid.NewGuid() };

        _cosmosClientWrapperMock
            .Setup(c => c.QueryFirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<FinalExam>, IQueryable<FinalExam>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinalExam?)null);

        // Act
        var result = await _repository.GetLearnerFinalExamByCourseIdAsync(courseId, questionIds, isCompleted: true, _cancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLearnerFinalExamQuestionOptionIdsAsync_ReturnsNullIfNoQuestionsExists()
    {
        var courseId = Guid.NewGuid();
        var expected = new FinalExam()
        {

            CourseId = courseId,
            FinalExamQuestions = null, 
            Created = DateTime.UtcNow,
            CreatedBy = _currentUserServiceMock.Object.UserId

        };

        _cosmosClientWrapperMock
            .Setup(c => c.QueryFirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<FinalExam>, IQueryable<FinalExam>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinalExam?)null);

        var result = await _repository.GetLearnerFinalExamQuestionAnswerIdsAsync(courseId, _cancellationToken);

        Assert.Null(result);
    }

    

    [Fact]
    public async Task GetFinalExamSettingsAsync_ReturnsFinalExam_WhenExistsAndQuestionsDisplayedPerExamIsAtLeast2()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var expected = new FinalExam
        {
            CourseId = courseId,
            MinimumPercentageToPass = 80,
            Duration = new Duration { Hours = 1, Minutes = 30 },
            QuestionsDisplayedPerExam = 5,
            Created = DateTime.UtcNow
        };

        _cosmosClientWrapperMock
            .Setup(x => x.QueryFirstOrDefaultAsync<FinalExam>(
                It.IsAny<Func<IQueryable<FinalExam>, IQueryable<FinalExam>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _repository.GetFinalExamSettingsAsync(courseId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.CourseId, result.CourseId);
        Assert.Equal(expected.MinimumPercentageToPass, result.MinimumPercentageToPass);
        Assert.Equal(expected.Duration, result.Duration);
        Assert.Equal(expected.QuestionsDisplayedPerExam, result.QuestionsDisplayedPerExam);
    }

    
    [Fact]
    public async Task GetFinalExamSettingsAsync_ReturnsProjectedFinalExam_WhenFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var expected = new FinalExam
        {
            CourseId = courseId,
            MinimumPercentageToPass = 85,
            Duration = new Duration { Hours = 1, Minutes = 15 },
            QuestionsDisplayedPerExam = 3,
            Created = DateTime.UtcNow
        };

        _cosmosClientWrapperMock
            .Setup(x => x.QueryFirstOrDefaultAsync<FinalExam>(
                It.IsAny<Func<IQueryable<FinalExam>, IQueryable<FinalExam>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _repository.GetFinalExamSettingsAsync(courseId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.CourseId, result.CourseId);
        Assert.Equal(expected.MinimumPercentageToPass, result.MinimumPercentageToPass);
        Assert.Equal(expected.Duration, result.Duration);
        Assert.Equal(expected.QuestionsDisplayedPerExam, result.QuestionsDisplayedPerExam);
        Assert.Equal(expected.Created, result.Created);
    }

    [Fact]
    public async Task GetFinalExamSettingsAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        _cosmosClientWrapperMock
            .Setup(x => x.QueryFirstOrDefaultAsync<FinalExam>(
                It.IsAny<Func<IQueryable<FinalExam>, IQueryable<FinalExam>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinalExam?)null);

        // Act
        var result = await _repository.GetFinalExamSettingsAsync(courseId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}