using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Commands.LearningContent;

public class UpdateLearningContentCommandHandlerTests
{
    private readonly Mock<ILearningContentService> _serviceMock = new();
    private readonly Mock<ILearningContentRepository> _repositoryMock = new();
    private readonly UpdateLearningContentCommand.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private static readonly Guid ValidSectionId = Guid.NewGuid();
    private static readonly Guid ValidLearningContentId = Guid.NewGuid();
    private static readonly Guid ValidCourseId = Guid.NewGuid();
    private const string ValidSectionName = "Test Section";

    private static readonly JsonPatchDocument ValidSectionUpdate = new()
    {
        Operations =
        {
            new Operation<LearningContentDto>
            {
                op = "add",
                path = "/sections/-",
                value = new LearningContentSectionDto
                {
                    SectionId = ValidSectionId,
                    Name = new LocalizedStringDto
                    {
                        En = ValidSectionName
                    }
                }
            }
        }
    };

    private static readonly LearningContentDto ExistingLearningContentDto = new()
    {
        Id = ValidLearningContentId,
        CourseId = ValidCourseId,
        Sections = []
    };
    
    private static readonly LearningContentDto ValidExpectedResult = new()
    {
        Id = ValidLearningContentId,
        CourseId = ValidCourseId,
        Sections =
        [
            new()
            {
                SectionId = ValidSectionId,
                Name = new LocalizedStringDto { En = ValidSectionName },
                LearningObjects = []
            }
        ]
    };

    public UpdateLearningContentCommandHandlerTests()
    {
        _handler = new UpdateLearningContentCommand.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnLearningContentDto_WhenUpdateIsSuccessful()
    {
        _serviceMock
            .Setup(service => service.GetByCourseIdAsync(ValidCourseId, _cancellationToken))
            .ReturnsAsync(ExistingLearningContentDto);

        _serviceMock
            .Setup(service => service.UpdateLearningContentAsync(ValidCourseId, ValidSectionUpdate, ExistingLearningContentDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidExpectedResult);

        var contract = new UpdateLearningContentCommand.Contract 
        { 
            CourseId = ValidCourseId,
            Updates = ValidSectionUpdate
        };

        // Act
        var result = await _handler.Handle(contract, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidExpectedResult.Id, result.Id);
        Assert.Equal(ValidExpectedResult.CourseId, result.CourseId);
        
        // Verify sections
        Assert.NotNull(result.Sections);
        Assert.Single(result.Sections);
        var section = result.Sections.First();
        Assert.Equal(ValidSectionId, section.SectionId);
        Assert.Equal(ValidSectionName, section.Name.En);

        _serviceMock.Verify(
            service => service.UpdateLearningContentAsync(ValidCourseId, ValidSectionUpdate, ExistingLearningContentDto, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsInvalidOperationException_WhenLearningContentNotFound()
    {
        // Arrange
        Guid courseId = Guid.NewGuid();

        _repositoryMock
            .Setup(repo => repo.GetByCourseIdAsync(It.IsAny<Guid>(), _cancellationToken))
            .ThrowsAsync(new InvalidOperationException($"Learning content for course {courseId} not found."
                ));

        var contract = new UpdateLearningContentCommand.Contract
        {
            CourseId = courseId,
            Updates = ValidSectionUpdate
        };


        // Act
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(contract, _cancellationToken));

        // Assert
        Assert.Contains($"Learning content for course {courseId} not found", exception.Message);
    }
}