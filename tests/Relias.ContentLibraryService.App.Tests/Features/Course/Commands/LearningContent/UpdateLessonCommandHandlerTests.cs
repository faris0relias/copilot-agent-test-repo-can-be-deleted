using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Commands.LearningContent;

public class UpdateLessonCommandHandlerTests
{
    private readonly Mock<ILearningContentService> _serviceMock = new();
    private readonly UpdateLessonCommand.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private static readonly Guid ValidCourseId = Guid.NewGuid();
    private static readonly Guid ValidSectionId = Guid.NewGuid();
    private static readonly Guid ValidLearningContentId = Guid.NewGuid();
    private static readonly Guid ValidLearningObjectId = Guid.NewGuid();
    private static readonly string NewLessonName = "New Lesson name";
    private static readonly string NewLessonType = "file";

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
                        Name = new LocalizedStringDto { En = "Old Lesson name" },
                        LessonType = "video",
                        DurationMinutes = 15,
                        RequiredForCompletion = false,
                        RequiresAudio = true,
                        RequiresVideo = false,
                        OpensInNewTab = true,
                        ContentPath = "/file/old-video.mp4"
                    }
                ]
            }
        ]
    };

    private static readonly UpdateLessonDto UpdateLessonDto = new()
    {
        LearningObjectId = ValidLearningObjectId,
        LearningObjectType = LearningObjectType.Lesson,
        Name = new LocalizedStringDto { En = NewLessonName },
        LessonType = NewLessonType,
        DurationMinutes = 10,
        RequiredForCompletion = true,
        RequiresAudio = true,
        RequiresVideo = false,
        OpensInNewTab = true,
        ContentPath = "/file/video.mp4",
        FileName = "video.mp4",
        FileSize = "1024"
    };

    private static readonly LessonDto ExpectedResult = new()
    {
        LearningObjectId = ValidLearningObjectId,
        LearningObjectType = LearningObjectType.Lesson,
        Name = new LocalizedStringDto { En = NewLessonName },
        LessonType = NewLessonType,
        DurationMinutes = 10,
        RequiredForCompletion = true,
        RequiresAudio = true,
        RequiresVideo = false,
        OpensInNewTab = true,
        ContentPath = "/file/video.mp4"
    };

    public UpdateLessonCommandHandlerTests()
    {
        _handler = new UpdateLessonCommand.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnLearningContentDto_WhenLessonIsUpdatedSuccessfully()
    {
        _serviceMock
            .Setup(service => service.GetByCourseIdAsync(ValidCourseId, _cancellationToken))
            .ReturnsAsync(ExistingLearningContentDto);

        _serviceMock
            .Setup(service => service.UpdateLessonAsync(
                ValidCourseId,
                ValidSectionId,
                ValidLearningObjectId,
                UpdateLessonDto,
                ExistingLearningContentDto,
                It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(ExpectedResult);

        var command = new UpdateLessonCommand.Contract
        {
            CourseId = ValidCourseId,
            SectionId = ValidSectionId,
            LearningObjectId = ValidLearningObjectId,
            LessonDto = UpdateLessonDto
        };

        var result = await _handler.Handle(command, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(ExpectedResult.LearningObjectId, result.LearningObjectId);
        Assert.Equal(ExpectedResult.LearningObjectType, result.LearningObjectType);
        Assert.Equal(NewLessonName, result.Name.En);
        Assert.Equal(NewLessonType, result.LessonType);
        Assert.Equal(10, result.DurationMinutes);
        Assert.True(result.RequiredForCompletion);
        Assert.Equal("/file/video.mp4", result.ContentPath);

        _serviceMock.Verify(service =>
            service.UpdateLessonAsync(ValidCourseId, ValidSectionId, ValidLearningObjectId, UpdateLessonDto, ExistingLearningContentDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
