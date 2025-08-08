using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Commands.LearningContent;

public class DeleteLessonCommandHandlerTests
{
    private readonly Mock<ILearningContentService> _serviceMock = new();
    private readonly DeleteLessonCommand.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private static readonly Guid ValidCourseId = Guid.NewGuid();
    private static readonly Guid ValidSectionId = Guid.NewGuid();
    private static readonly Guid ValidLearningContentId = Guid.NewGuid();
    private static readonly Guid ValidLearningObjectId = Guid.NewGuid();
    private static readonly int ValidOrgId = 8;

    private static readonly LearningContentDto ExistingLearningContentDto = new()
    {
        Id = ValidLearningContentId,
        CourseId = ValidCourseId,
        Sections =
        [
            new LearningContentSectionDto
            {
                SectionId = ValidSectionId,
                Name = new LocalizedStringDto { En = "Section 1" },
                LearningObjects = 
                [
                    new LessonDto
                    {
                        LearningObjectId = ValidLearningObjectId,
                        LearningObjectType = LearningObjectType.Lesson,
                        Name = new LocalizedStringDto { En = "Lesson to Delete" },
                        LessonType = "file",
                        DurationMinutes = 10,
                        RequiredForCompletion = true,
                        RequiresAudio = false,
                        RequiresVideo = true,
                        OpensInNewTab = false,
                        ContentPath = "main"
                    }
                ]
            }
        ]
    };

    public DeleteLessonCommandHandlerTests()
    {
        _handler = new DeleteLessonCommand.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsDeleteLessonAsync()
    {
        // Arrange
        var request = new DeleteLessonCommand.Contract(ValidCourseId, ValidSectionId, ValidLearningObjectId, ValidOrgId);

        _serviceMock
            .Setup(s => s.GetByCourseIdAsync(ValidCourseId, _cancellationToken))
            .ReturnsAsync(ExistingLearningContentDto);

        _serviceMock
            .Setup(s => s.DeleteLessonAsync(ValidCourseId, ValidSectionId, ValidLearningObjectId, ValidOrgId, ExistingLearningContentDto, _cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(request, _cancellationToken);

        // Assert
        _serviceMock.Verify(s => s.GetByCourseIdAsync(ValidCourseId, _cancellationToken), Times.Once);
        _serviceMock.Verify(s => s.DeleteLessonAsync(ValidCourseId, ValidSectionId, ValidLearningObjectId, ValidOrgId, ExistingLearningContentDto, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_LearningContentNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new DeleteLessonCommand.Contract(ValidCourseId, ValidSectionId, ValidLearningObjectId, ValidOrgId);

        _serviceMock
            .Setup(s => s.GetByCourseIdAsync(ValidCourseId, _cancellationToken))
            .ReturnsAsync((LearningContentDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(request, _cancellationToken));
        Assert.Equal($"Learning content for course {ValidCourseId} not found.", exception.Message);
    }
}
