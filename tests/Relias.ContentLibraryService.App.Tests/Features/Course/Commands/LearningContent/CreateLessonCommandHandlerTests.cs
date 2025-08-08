using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Commands.LearningContent;

public class CreateLessonCommandHandlerTests
{
    private readonly Mock<ILearningContentService> _serviceMock = new();
    private readonly CreateLessonCommand.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private static readonly Guid ValidCourseId = Guid.NewGuid();
    private static readonly Guid ValidSectionId = Guid.NewGuid();
    private static readonly Guid ValidLearningContentId = Guid.NewGuid();
    private static readonly Guid ValidLearningObjectId = Guid.NewGuid();
    private const string ValidLessonName = "Valid Lesson";
    private const string ValidLessonType = "file";

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
                LearningObjects = []
            }
        ]
    };

    private static readonly CreateLessonDto ValidNewLessonDto = new()
    {
        Name = new LocalizedStringDto { En = ValidLessonName },
        LessonType = ValidLessonType,
        DurationMinutes = 10,
        RequiredForCompletion = true,
        RequiresAudio = false,
        RequiresVideo = true,
        OpensInNewTab = false,
        ContentPath = "/file/video.mp4"
    };

    private static readonly LessonDto ExpectedResult = new()
    {
        LearningObjectId = ValidLearningObjectId,
        LearningObjectType = LearningObjectType.Lesson,
        Name = new LocalizedStringDto { En = ValidLessonName },
        LessonType = ValidLessonType,
        DurationMinutes = 10,
        RequiredForCompletion = true,
        RequiresAudio = false,
        RequiresVideo = true,
        OpensInNewTab = false,
        ContentPath = "/file/video.mp4"
    };

    public CreateLessonCommandHandlerTests()
    {
        _handler = new CreateLessonCommand.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnLearningContentDto_WhenLessonIsCreatedSuccessfully()
    {
        _serviceMock
            .Setup(service => service.GetByCourseIdAsync(ValidCourseId, _cancellationToken))
            .ReturnsAsync(ExistingLearningContentDto);

        _serviceMock
            .Setup(service => service.CreateLessonAsync(
                ValidCourseId,
                ValidSectionId,
                ValidNewLessonDto,
                ExistingLearningContentDto,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExpectedResult);

        var command = new CreateLessonCommand.Contract
        {
            CourseId = ValidCourseId,
            SectionId = ValidSectionId,
            LessonDto = ValidNewLessonDto
        };

        var result = await _handler.Handle(command, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(ExpectedResult.LearningObjectId, result.LearningObjectId);
        Assert.Equal(ExpectedResult.LearningObjectType, result.LearningObjectType);
        Assert.Equal(ValidLessonName, result.Name.En);
        Assert.Equal(ValidLessonType, result.LessonType);
        Assert.Equal(10, result.DurationMinutes);

        _serviceMock.Verify(service =>
            service.CreateLessonAsync(ValidCourseId, ValidSectionId, ValidNewLessonDto, ExistingLearningContentDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
