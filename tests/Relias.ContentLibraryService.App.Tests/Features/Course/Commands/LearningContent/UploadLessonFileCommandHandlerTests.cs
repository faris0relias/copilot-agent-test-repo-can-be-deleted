using Microsoft.AspNetCore.Http;
using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Commands.LearningContent;

public class UploadLessonFileCommandHandlerTests
{
    private readonly Mock<ILearningContentService> _serviceMock = new();
    private readonly Mock<IFormFile> _formFileMock = new();
    private readonly UploadLessonFileCommand.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private static readonly Guid ValidCourseId = Guid.NewGuid();
    private static readonly Guid ValidLearningObjectId = Guid.NewGuid();
    private const int ValidOrganizationId = 123;
    private const string ValidUploadId = "upload-123";
    private const int ValidChunkIndex = 0;
    private const int ValidTotalChunks = 3;
    private const string ValidFileName = "lesson-video.mp4";

    public UploadLessonFileCommandHandlerTests()
    {
        _handler = new UploadLessonFileCommand.Handler(_serviceMock.Object);
        SetupFormFileMock();
    }

    private void SetupFormFileMock()
    {
        _formFileMock.Setup(f => f.Length).Returns(1024);
        _formFileMock.Setup(f => f.FileName).Returns(ValidFileName);
        _formFileMock.Setup(f => f.ContentType).Returns("video/mp4");
    }

    [Fact]
    public async Task Handle_ShouldCompleteSuccessfully_WhenServiceCompletes()
    {
        _serviceMock.Setup(service => service.UploadLessonFileAsync(
            ValidOrganizationId,
            ValidCourseId,
            ValidLearningObjectId,
            _formFileMock.Object,
            ValidUploadId,
            ValidChunkIndex,
            ValidTotalChunks,
            ValidFileName,
            _cancellationToken))
            .Returns(Task.CompletedTask);

        var command = new UploadLessonFileCommand.Contract
        {
            CourseId = ValidCourseId,
            LearningObjectId = ValidLearningObjectId,
            OrganizationId = ValidOrganizationId,
            Chunk = _formFileMock.Object,
            UploadId = ValidUploadId,
            ChunkIndex = ValidChunkIndex,
            TotalChunks = ValidTotalChunks,
            FileName = ValidFileName
        };

        await _handler.Handle(command, _cancellationToken);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 5)]
    [InlineData(2, 3)]
    public async Task Handle_ShouldHandleDifferentChunkIndexes_Correctly(int chunkIndex, int totalChunks)
    {
        // Arrange
        var command = new UploadLessonFileCommand.Contract
        {
            CourseId = ValidCourseId,
            LearningObjectId = ValidLearningObjectId,
            OrganizationId = ValidOrganizationId,
            Chunk = _formFileMock.Object,
            UploadId = ValidUploadId,
            ChunkIndex = chunkIndex,
            TotalChunks = totalChunks,
            FileName = ValidFileName
        };

        await _handler.Handle(command, _cancellationToken);

        _serviceMock.Verify(service => service.UploadLessonFileAsync(
            ValidOrganizationId,
            ValidCourseId,
            ValidLearningObjectId,
            _formFileMock.Object,
            ValidUploadId,
            chunkIndex,
            totalChunks,
            ValidFileName,
            _cancellationToken), Times.Once);
    }
}
