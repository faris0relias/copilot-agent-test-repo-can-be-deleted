using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Layouts;
using Microsoft.Extensions.Logging;
using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.App.Services;
using Relias.ContentLibraryService.Common.Constants;
using Relias.ContentLibraryService.Common.Helpers;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Relias.ContentLibraryService.Infra.Repositories;
using System;
using System.Text;
using ValidationException = Relias.ContentLibraryService.Common.Exceptions.ValidationException;

namespace Relias.ContentLibraryService.App.Tests.Services;

public class LearningContentServiceTests
{
    private readonly Mock<IMainBlobStorageRepository> _mockBlobRepository = new();
    private readonly Mock<ILearningContentRepository> _mockRepository = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ILogger<LearningContentService>> _mockLogger = new();
    private readonly LearningContentService _service;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private static readonly Guid ValidSectionId = Guid.NewGuid();
    private const string ValidSectionName = "Test Section";


    public LearningContentServiceTests()
    {
        _service = new LearningContentService(_mockRepository.Object, _mockBlobRepository.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task InitializeByCourseIdAsync_WhenExistingContentExists_ReturnsMappedDto()
    {
        Guid courseId = Guid.NewGuid();

        LearningContent existingContent = new()
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSection()
                {
                    SectionId = Guid.NewGuid(),
                    Name = new LocalizedString { En = "Section 1" },
                    LearningObjects = []
                }
            ]
        };

        LearningContentDto expectedDto = new()
        {
            Id = existingContent.Id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto()
                {
                    SectionId = existingContent.Sections[0].SectionId,
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects = []
                }
            ]
        };

        _mockRepository
            .Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(existingContent);

        _mockMapper
            .Setup(mapper => mapper.Map<LearningContentDto>(existingContent))
            .Returns(expectedDto);

        LearningContentDto result = await _service.InitializeByCourseIdAsync(courseId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Single(result.Sections);
        Assert.Equal("Section 1", result.Sections[0].Name.En);

        _mockRepository.Verify(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken), Times.Once);
        _mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<LearningContent>(), _cancellationToken), Times.Never);
        _mockMapper.Verify(mapper => mapper.Map<LearningContentDto>(existingContent), Times.Once);
    }

    [Fact]
    public async Task InitializeByCourseIdAsync_WhenNoExistingContent_CreatesAndReturnsDto()
    {
        Guid courseId = Guid.NewGuid();

        _mockRepository
            .Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync((LearningContent?)null);

        LearningContent createdContent = new()
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSection() {
                    SectionId = Guid.NewGuid(),
                    Name = new LocalizedString { En = "Section 1" },
                    LearningObjects = []
                }
            ]
        };

        LearningContentDto expectedDto = new()
        {
            Id = createdContent.Id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto()
                {
                    SectionId = createdContent.Sections[0].SectionId,
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects = []
                }
            ]
        };

        _mockRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<LearningContent>(), _cancellationToken))
            .ReturnsAsync(createdContent);

        _mockMapper
            .Setup(mapper => mapper.Map<LearningContentDto>(createdContent))
            .Returns(expectedDto);

        LearningContentDto result = await _service.InitializeByCourseIdAsync(courseId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.CourseId, result.CourseId);
        Assert.Single(result.Sections);

        _mockRepository.Verify(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken), Times.Once);
        _mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<LearningContent>(), _cancellationToken), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<LearningContentDto>(createdContent), Times.Once);
    }

    [Fact]
    public async Task GetByCourseIdAsync_WhenContentExists_ReturnsMappedDto()
    {
        Guid courseId = Guid.NewGuid();

        LearningContent entity = new() { Id = Guid.NewGuid(), CourseId = courseId };
        LearningContentDto dto = new() { Id = entity.Id, CourseId = entity.CourseId };

        _mockRepository
            .Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(entity);

        _mockMapper
            .Setup(mapper => mapper.Map<LearningContentDto>(entity))
            .Returns(dto);

        LearningContentDto? result = await _service.GetByCourseIdAsync(courseId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.CourseId, result.CourseId);
    }

    [Fact]
    public async Task GetByCourseIdAsync_WhenContentDoesNotExist_ReturnsNull()
    {
        Guid courseId = Guid.NewGuid();

        _mockRepository
            .Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync((LearningContent?)null);

        LearningContentDto? result = await _service.GetByCourseIdAsync(courseId, _cancellationToken);

        Assert.Null(result);

        _mockMapper.Verify(mapper => mapper.Map<LearningContentDto>(null), Times.Once);
    }

    #region SectionPatchTests

    [Fact]
    public async Task UpdateLearningContentAsync_WhenAddingNewSection_AddsSuccessfully()
    {
        Guid id = Guid.NewGuid();
        Guid courseId = Guid.NewGuid();

        LearningContent existing = new()
        {
            Id = id,
            CourseId = courseId,
            Sections = []
        };

        LearningContentDto existingDto = new()
        {
            Id = id,
            CourseId = courseId,
            Sections = []
        };

        JsonPatchDocument patchDto = new()
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

        var updated = new LearningContent
        {
            Id = id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSection
                {
                    SectionId = ValidSectionId,
                    Name = new LocalizedString { En = ValidSectionName },
                    LearningObjects = []
                }
            ]
        };

        var expectedDto = new LearningContentDto
        {
            Id = existingDto.Id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = ValidSectionId,
                    Name = new LocalizedStringDto { En = ValidSectionName },
                    LearningObjects = []
                }
            ]
        };

        SetupMocksForLearningContentPatchFlow(courseId, existing, existingDto, updated, expectedDto);

        LearningContentDto result = await _service.UpdateLearningContentAsync(courseId, patchDto, existingDto, _cancellationToken);

        Assert.NotNull(result);
        Assert.Single(result.Sections);
        Assert.Equal(ValidSectionId, result.Sections[0].SectionId);
        Assert.Equal(ValidSectionName, result.Sections[0].Name.En);

        _mockRepository.Verify(repo => repo.UpdateAsync(updated, _cancellationToken), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<LearningContentDto>(updated), Times.Once);
    }

    [Fact]
    public async Task UpdateLearningContentAsync_WhenRemovingSectionThatDoesNotExist_ThrowsJsonPatchException()
    {
        Guid courseId = Guid.NewGuid();

        LearningContent existing = new()
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections = []
        };
        LearningContentDto existingDto = new()
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections = []
        };

        JsonPatchDocument patchDto = new()
        {
            Operations =
            {
                new Operation<LearningContentDto>
                {
                    op = "remove",
                    path = "/sections/0"
                }
            }
        };

        _mockRepository.Setup(r => r.GetByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(existing);
        _mockMapper.Setup(m => m.Map<LearningContentDto>(existing)).Returns(existingDto);

        JsonPatchException ex = await Assert.ThrowsAsync<JsonPatchException>(() =>
            _service.UpdateLearningContentAsync(courseId, patchDto, existingDto, _cancellationToken));

        Assert.Contains("out of bounds of the array size", ex.Message);
    }

    [Fact]
    public async Task UpdateLearningContentAsync_WhenAddingSectionWithMissingName_ThrowsValidationException()
    {
        Guid courseId = Guid.NewGuid();
        LearningContent existing = new() { CourseId = courseId, Sections = [] };
        LearningContentDto existingDto = new() { CourseId = courseId, Sections = [] };

        JsonPatchDocument patchDto = new()
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
                            En = string.Empty
                        }
                    }
                }
            }
        };

        _mockRepository.Setup(r => r.GetByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(existing);
        _mockMapper.Setup(m => m.Map<LearningContentDto>(existing)).Returns(existingDto);

        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpdateLearningContentAsync(courseId, patchDto, existingDto, _cancellationToken));

        string[] exception = ex.Errors
            .Select(e => e.Value)
            .FirstOrDefault()!;

        string exceptionMessage = exception.Single();

        Assert.Contains($"Name.En cannot be empty - Section Id: {ValidSectionId}", exceptionMessage);
    }

    [Fact]
    public async Task UpdateLearningContentAsync_WhenRemovingNewSection_RemovesSuccessfully()
    {
        Guid id = Guid.NewGuid();
        Guid courseId = Guid.NewGuid();
        Guid sectionId = Guid.NewGuid();

        LearningContent existing = new()
        {
            Id = id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSection
                {
                    SectionId = ValidSectionId,
                    Name = new LocalizedString { En = "Section 1" },
                    LearningObjects = []
                },
                new LearningContentSection
                {
                    SectionId = sectionId,
                    Name = new LocalizedString { En = "Section 2" },
                    LearningObjects = []
                }
            ]
        };

        LearningContentDto existingDto = new()
        {
            Id = id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = ValidSectionId,
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects = []
                },
                new LearningContentSectionDto
                {
                    SectionId = sectionId,
                    Name = new LocalizedStringDto { En = "Section 2" },
                    LearningObjects = []
                }
            ]
        };

        JsonPatchDocument patchDto = new()
        {
            Operations =
            {
                new Operation<LearningContentDto>
                {
                    op = "remove",
                    path = "/sections/0"
                }
            }
        };

        LearningContent updated = new()
        {
            Id = id,
            CourseId = courseId,
            Sections = [
                new LearningContentSection
                {
                    SectionId = ValidSectionId,
                    Name = new LocalizedString { En = "Section 1" },
                    LearningObjects = []
                }
            ]
        };

        LearningContentDto expectedDto = new()
        {
            Id = existing.Id,
            CourseId = courseId,
            Sections = [
                new LearningContentSectionDto
                {
                    SectionId = ValidSectionId,
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects = []
                }
            ]
        };

        SetupMocksForLearningContentPatchFlow(courseId, existing, existingDto, updated, expectedDto);

        LearningContentDto result = await _service.UpdateLearningContentAsync(courseId, patchDto, existingDto, _cancellationToken);

        Assert.NotNull(result);
        Assert.Single(result.Sections);
        Assert.Equal(ValidSectionId, result.Sections[0].SectionId);

        _mockRepository.Verify(repo => repo.UpdateAsync(updated, _cancellationToken), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<LearningContentDto>(updated), Times.Once);
    }

    [Fact]
    public async Task UpdateLearningContentAsync_WhenReplacingNewSection_ReplacesSuccessfully()
    {
        Guid id = Guid.NewGuid();
        Guid courseId = Guid.NewGuid();
        Guid sectionId = Guid.NewGuid();

        LearningContent existing = new()
        {
            Id = id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSection
                {
                    SectionId = ValidSectionId,
                    Name = new LocalizedString { En = ValidSectionName },
                    LearningObjects = []
                }
            ]
        };

        LearningContentDto existingDto = new()
        {
            Id = id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = sectionId,
                    Name = new LocalizedStringDto { En = ValidSectionName },
                    LearningObjects = []
                }
            ]
        };

        JsonPatchDocument patchDto = new()
        {
            Operations =
            {
                new Operation<LearningContentDto>
                {
                    op = "replace",
                    path = "/sections/0",
                    value = new LearningContentSectionDto
                    {
                        SectionId = ValidSectionId,
                        Name = new LocalizedStringDto
                        {
                            En = "Updated Section"
                        }
                    }
                }
            }
        };

        LearningContent updated = new()
        {
            Id = id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSection
                {
                    SectionId = ValidSectionId,
                    Name = new LocalizedString { En = "Updated Section" },
                    LearningObjects = []
                }
            ]
        };

        LearningContentDto expectedDto = new()
        {
            Id = existing.Id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = ValidSectionId,
                    Name = new LocalizedStringDto { En = "Updated Section" },
                    LearningObjects = []
                }
            ]
        };
        SetupMocksForLearningContentPatchFlow(courseId, existing, existingDto, updated, expectedDto);

        LearningContentDto result = await _service.UpdateLearningContentAsync(courseId, patchDto, existingDto, _cancellationToken);

        Assert.NotNull(result);
        Assert.Single(result.Sections);
        Assert.Equal(ValidSectionId, result.Sections[0].SectionId);
        Assert.Equal("Updated Section", result.Sections[0].Name.En);

        _mockRepository.Verify(repo => repo.UpdateAsync(updated, _cancellationToken), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<LearningContentDto>(updated), Times.Once);
    }

    [Fact]
    public async Task UpdateLearningContentAsync_WhenSectionNotFound_ThrowsInvalidOperationException()
    {
        Guid courseId = Guid.NewGuid();

        LearningContent existing = new()
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections = []
        };

        LearningContentDto existingDto = new()
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections = []
        };

        JsonPatchDocument patchDto = new()
        {
            Operations =
            {
                new Operation<LearningContentDto>
                {
                    op = "replace",
                    path = "/sections/0",
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

        _mockRepository
            .Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(existing);
        _mockMapper.Setup(m => m.Map<LearningContentDto>(existing)).Returns(existingDto);

        JsonPatchException exception = await Assert.ThrowsAsync<JsonPatchException>(
            () => _service.UpdateLearningContentAsync(courseId, patchDto, existingDto, _cancellationToken));

        Assert.Contains("out of bounds of the array size", exception.Message);
    }


    [Fact]
    public async Task UpdateLearningContentAsync_WhenMovingSection_MovesSuccessfully()
    {
        Guid id = Guid.NewGuid();
        Guid courseId = Guid.NewGuid();
        Guid firstSectionId = Guid.NewGuid();
        Guid movedSectionId = Guid.NewGuid();

        LearningContent existing = new()
        {
            Id = id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSection
                {
                    SectionId = firstSectionId,
                    Name = new LocalizedString { En = ValidSectionName },
                    LearningObjects = []
                },
                new LearningContentSection
                {
                    SectionId = movedSectionId,
                    Name = new LocalizedString { En = "Moving Section" },
                    LearningObjects = []
                }
            ]
        };

        LearningContentDto existingDto = new()
        {
            Id = id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = firstSectionId,
                    Name = new LocalizedStringDto { En = ValidSectionName },
                    LearningObjects = []
                },
                new LearningContentSectionDto
                {
                    SectionId = movedSectionId,
                    Name = new LocalizedStringDto() { En = "Moving Section" },
                    LearningObjects = []
                }
            ]
        };

        JsonPatchDocument patchDto = new()
        {
            Operations =
            {
                new Operation<LearningContentDto>
                {
                    op = "move",
                    from = "/sections/1",
                    path = "/sections/0"
                }
            }
        };

        LearningContent updated = new()
        {
            Id = id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSection
                {
                    SectionId = movedSectionId,
                    Name = new LocalizedString { En = "Moving Section" },
                    LearningObjects = []
                },
                new LearningContentSection
                {
                    SectionId = firstSectionId,
                    Name = new LocalizedString { En = ValidSectionName },
                    LearningObjects = []
                }

            ]
        };



        LearningContentDto expectedDto = new()
        {
            Id = existing.Id,
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = movedSectionId,
                    Name = new LocalizedStringDto() { En = "Moving Section" },
                    LearningObjects = []
                },
                new LearningContentSectionDto
                {
                    SectionId = firstSectionId,
                    Name = new LocalizedStringDto { En = ValidSectionName },
                    LearningObjects = []
                }
            ]
        };

        SetupMocksForLearningContentPatchFlow(courseId, existing, existingDto, updated, expectedDto);

        LearningContentDto result = await _service.UpdateLearningContentAsync(courseId, patchDto, existingDto, _cancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Sections);
        Assert.Equal(movedSectionId, result.Sections[0].SectionId);
        Assert.Equal("Moving Section", result.Sections[0].Name.En);

        _mockRepository.Verify(repo => repo.UpdateAsync(updated, _cancellationToken), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<LearningContentDto>(updated), Times.Once);
    }

    [Fact]
    public async Task UpdateLearningContentAsync_WhenInvalidMoveSection_ThrowsJsonPatchException()
    {
        Guid id = Guid.NewGuid();
        Guid courseId = Guid.NewGuid();

        LearningContent existing = new()
        {
            Id = id,
            CourseId = courseId,
            Sections = []
        };

        LearningContentDto existingDto = new()
        {
            Id = id,
            CourseId = courseId,
            Sections = []
        };

        JsonPatchDocument patchDto = new()
        {
            Operations =
            {
                new Operation<LearningContentDto>
                {
                    op = "move",
                    from = "/sections/1",
                    path = "/sections/0"
                }
            }
        };

        _mockRepository
            .Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(existing);
        _mockMapper.Setup(m => m.Map<LearningContentDto>(existing)).Returns(existingDto);

        JsonPatchException exception = await Assert.ThrowsAsync<JsonPatchException>(
            () => _service.UpdateLearningContentAsync(courseId, patchDto, existingDto, _cancellationToken));

        Assert.Contains("out of bounds of the array size", exception.Message);
    }

    #endregion

    #region Create Lesson Tests

    [Fact]
    public async Task CreateLessonAsync_WhenValidInputs_AddsLessonToCorrectSection()
    {
        var courseId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
            {
                SectionId = sectionId,
                Name = new LocalizedStringDto { En = "Section 1" },
                LearningObjects = []
            }
            ]
        };

        var createLessonDto = new CreateLessonDto
        {
            Name = new LocalizedStringDto { En = "Lesson 1" },
            LessonType = "file",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = "/file/lesson.mp4"
        };

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken))
            .ReturnsAsync(new LearningContent());

        _mockMapper
            .Setup(m => m.Map<LearningContent>(It.IsAny<LearningContentDto>()))
            .Returns(new LearningContent());

        _mockMapper
            .Setup(m => m.Map<LearningContentDto>(It.IsAny<LearningContent>()))
            .Returns(new LearningContentDto());

        var result = await _service.CreateLessonAsync(courseId, sectionId, createLessonDto, existingDto, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(LearningObjectType.Lesson, result.LearningObjectType);
        Assert.Equal(createLessonDto.Name.En, result.Name.En);
        Assert.Equal(createLessonDto.LessonType, result.LessonType);
        Assert.Equal(createLessonDto.DurationMinutes, result.DurationMinutes);
        Assert.Equal(createLessonDto.ContentPath, result.ContentPath);
        Assert.NotEqual(Guid.Empty, result.LearningObjectId);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken), Times.Once);
    }

    #endregion

    #region Update Lesson Tests

    [Fact]
    public async Task UpdateLessonAsync_WhenValidInputs_UpdateLessonSuccessfully()
    {
        var courseId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var learningObjectId = Guid.NewGuid();
        var orgId = new Random().Next(1, 100001).ToString();

        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
            {
                SectionId = sectionId,
                Name = new LocalizedStringDto { En = "Section 1" },
                LearningObjects =
                [
                    new LessonDto
                    {
                        LearningObjectId = learningObjectId,
                        LearningObjectType = LearningObjectType.Lesson,
                        Name = new LocalizedStringDto { En = "Old Lesson" },
                        LessonType = "file",
                        DurationMinutes = 5,
                        RequiredForCompletion = true,
                        RequiresAudio = false,
                        RequiresVideo = false,
                        OpensInNewTab = false,
                        ContentPath = "/file/old_lesson.mp4"
                    }
                ]
            }
            ]
        };

        var updateLessonDto = new UpdateLessonDto
        {
            LearningObjectId = learningObjectId,
            LearningObjectType = LearningObjectType.Lesson,
            Name = new LocalizedStringDto { En = "Lesson 1" },
            LessonType = "file",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = "/file/lesson.mp4"
        };

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken))
            .ReturnsAsync(new LearningContent());

        _mockMapper
            .Setup(m => m.Map<LearningContent>(It.IsAny<LearningContentDto>()))
            .Returns(new LearningContent());

        _mockMapper
            .Setup(m => m.Map<LearningContentDto>(It.IsAny<LearningContent>()))
            .Returns(new LearningContentDto());

        var result = await _service.UpdateLessonAsync(courseId, sectionId, learningObjectId, updateLessonDto, existingDto, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(LearningObjectType.Lesson, result.LearningObjectType);
        Assert.Equal(updateLessonDto.Name.En, result.Name.En);
        Assert.Equal(updateLessonDto.LessonType, result.LessonType);
        Assert.Equal(updateLessonDto.DurationMinutes, result.DurationMinutes);
        Assert.Equal(updateLessonDto.ContentPath, result.ContentPath);
        Assert.NotEqual(Guid.Empty, result.LearningObjectId);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateLessonAsync_WhenLessonDoesNotExist_ThrowsInvalidOperationException()
    {
        Guid courseId = Guid.NewGuid();
        Guid sectionId = Guid.NewGuid();
        Guid lessonId = Guid.NewGuid();

        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
           [
               new LearningContentSectionDto
                {
                    SectionId = sectionId,
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects = []
                }
           ]
        };

        var updatedLessonDto = new UpdateLessonDto
        {
            LearningObjectId = lessonId,
            Name = new LocalizedStringDto { En = "Updated Lesson Name" },
            ContentPath = "new/path",
            LessonType = "file"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.UpdateLessonAsync(courseId, sectionId, lessonId, updatedLessonDto, existingDto, _cancellationToken));
    }

    [Fact]
    public async Task UpdateLessonAsync_WhenLessonTypeFileDeleteFileTrue_DeletesBlobFile_Success()
    {
        // Arrange
        var containerName = "main";
        var orgId = new Random().Next(1, 100001).ToString();
        var courseId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var learningObjectId = Guid.NewGuid();
        var lessonName = new LocalizedStringDto { En = "Lesson to Delete" };
        var fileName = "lessonToDelete.mp4";
        var newFileName = "lessonToAdd.mp4";
        var contentPath = $"{orgId}/{courseId}/{learningObjectId}/";
        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = sectionId,
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects =
                    [
                        new LessonDto
                        {
                            LearningObjectId = learningObjectId,
                            LearningObjectType = LearningObjectType.Lesson,
                            Name = lessonName,
                            LessonType = "file",
                            DurationMinutes = 10,
                            RequiredForCompletion = true,
                            RequiresAudio = false,
                            RequiresVideo = false,
                            OpensInNewTab = false,
                            ContentPath = containerName,
                            FileName = fileName,
                            FileSize = "10"
                        }
                    ]
                }
            ]
        };

        var updateLessonDto = new UpdateLessonDto
        {
            LearningObjectId = learningObjectId,
            LearningObjectType = LearningObjectType.Lesson,
            Name = lessonName,
            LessonType = "file",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = containerName,
            FileName = newFileName,
            FileSize = "10",
            OrgId = orgId,
            DeleteFile = true
        };

        _mockBlobRepository.Setup(r => r.RemoveBlobAsync(containerName, contentPath, fileName))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken))
            .ReturnsAsync(new LearningContent());
        _mockMapper
            .Setup(m => m.Map<LearningContent>(It.IsAny<LearningContentDto>()))
            .Returns(new LearningContent());
        _mockMapper
            .Setup(m => m.Map<LearningContentDto>(It.IsAny<LearningContent>()))
            .Returns(new LearningContentDto());

        // Act
        var result = await _service.UpdateLessonAsync(courseId, sectionId, learningObjectId, updateLessonDto, existingDto, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        result.Should().BeEquivalentTo(updateLessonDto, options => options
            .Excluding(dto => dto.ContentPath)
            .Excluding(dto => dto.FileName)
            .Excluding(dto => dto.FileSize)
            .Excluding(dto => dto.OrgId)
            .Excluding(dto => dto.DeleteFile));
        result.FileName.Should().NotBeEmpty().Equals(newFileName);
        result.ContentPath.Should().NotBeEmpty();
        result.FileSize.Should().NotBeEmpty();
        _mockBlobRepository.Verify(r => r.RemoveBlobAsync(containerName, contentPath, fileName), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken), Times.Once);
    }


    [Fact]
    public async Task UpdateLessonAsync_WhenLessonTypeFileDeleteFileTrue_ContentPathHasContainerNameAndFileName_DeletesBlobFile_Success()
    {
        // Arrange
        var containerName = "main";
        var orgId = new Random().Next(1, 100001).ToString();
        var courseId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var learningObjectId = Guid.NewGuid();
        var lessonName = new LocalizedStringDto { En = "Lesson to Delete" };
        var fileName = "lessonToDelete.mp4";
        var newFileName = "lessonToAdd.mp4";
        var contentPath = $"{orgId}/{courseId}/{learningObjectId}/";
        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = sectionId,
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects =
                    [
                        new LessonDto
                        {
                            LearningObjectId = learningObjectId,
                            LearningObjectType = LearningObjectType.Lesson,
                            Name = lessonName,
                            LessonType = "file",
                            DurationMinutes = 10,
                            RequiredForCompletion = true,
                            RequiresAudio = false,
                            RequiresVideo = false,
                            OpensInNewTab = false,
                            ContentPath = containerName,
                            FileName = fileName,
                            FileSize = "10"
                        }
                    ]
                }
            ]
        };

        var updateLessonDto = new UpdateLessonDto
        {
            LearningObjectId = learningObjectId,
            LearningObjectType = LearningObjectType.Lesson,
            Name = lessonName,
            LessonType = "file",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = containerName,
            FileName = newFileName,
            FileSize = "10",
            OrgId = orgId,
            DeleteFile = true
        };

        _mockBlobRepository.Setup(r => r.RemoveBlobAsync(containerName, contentPath, fileName))
            .ReturnsAsync(true);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken))
            .ReturnsAsync(new LearningContent());
        _mockMapper
            .Setup(m => m.Map<LearningContent>(It.IsAny<LearningContentDto>()))
            .Returns(new LearningContent());
        _mockMapper
            .Setup(m => m.Map<LearningContentDto>(It.IsAny<LearningContent>()))
            .Returns(new LearningContentDto());

        // Act
        var result = await _service.UpdateLessonAsync(courseId, sectionId, learningObjectId, updateLessonDto, existingDto, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        result.Should().BeEquivalentTo(updateLessonDto, options => options
            .Excluding(dto => dto.ContentPath)
            .Excluding(dto => dto.FileName)
            .Excluding(dto => dto.FileSize)
            .Excluding(dto => dto.OrgId)
            .Excluding(dto => dto.DeleteFile));
        result.FileName.Should().NotBeEmpty().Equals(newFileName);
        result.ContentPath.Should().NotBeEmpty();
        result.FileSize.Should().NotBeEmpty();
        _mockBlobRepository.Verify(r => r.RemoveBlobAsync(containerName, contentPath, fileName), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateLessonAsync_MultipleSections_WhenLessonTypeFileDeleteFileTrue_DeletesBlobFile_Success()
    {
        // Arrange
        var containerName = "main";
        var orgId = new Random().Next(1, 100001).ToString();
        var courseId = Guid.NewGuid();
        var sectionId1 = Guid.NewGuid();
        var sectionId2 = Guid.NewGuid();
        var learningObjectId1 = Guid.NewGuid();
        var learningObjectId2 = Guid.NewGuid();
        var lessonName = new LocalizedStringDto { En = "Lesson to Delete" };
        var fileName = "lessonToDelete.mp4";
        var newFileName = "lessonToAdd.mp4";
        var contentPath = $"{orgId}/{courseId}/{learningObjectId1}/";
        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = sectionId1,
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects =
                    [
                        new LessonDto
                        {
                            LearningObjectId = learningObjectId1,
                            LearningObjectType = LearningObjectType.Lesson,
                            Name = lessonName,
                            LessonType = "file",
                            DurationMinutes = 10,
                            RequiredForCompletion = true,
                            RequiresAudio = false,
                            RequiresVideo = false,
                            OpensInNewTab = false,
                            ContentPath = containerName,
                            FileName = fileName,
                            FileSize = "10"
                        }
                    ]
                },
                new LearningContentSectionDto
                {
                    SectionId = sectionId2,
                    Name = new LocalizedStringDto { En = "Section 2" },
                    LearningObjects =
                    [
                        new LessonDto
                        {
                            LearningObjectId = learningObjectId2,
                            LearningObjectType = LearningObjectType.Lesson,
                            Name = lessonName,
                            LessonType = "file",
                            DurationMinutes = 10,
                            RequiredForCompletion = true,
                            RequiresAudio = false,
                            RequiresVideo = false,
                            OpensInNewTab = false,
                            ContentPath = contentPath,
                            FileName = fileName,
                            FileSize = "10"
                        }
                    ]
                }
            ]
        };

        var updateLessonDto = new UpdateLessonDto
        {
            LearningObjectId = learningObjectId1,
            LearningObjectType = LearningObjectType.Lesson,
            Name = lessonName,
            LessonType = "file",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = containerName,
            FileName = newFileName,
            FileSize = "10",
            OrgId = orgId,
            DeleteFile = true
        };

        _mockBlobRepository.Setup(r => r.RemoveBlobAsync(containerName, contentPath, fileName))
            .ReturnsAsync(true);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken))
            .ReturnsAsync(new LearningContent());
        _mockMapper
            .Setup(m => m.Map<LearningContent>(It.IsAny<LearningContentDto>()))
            .Returns(new LearningContent());
        _mockMapper
            .Setup(m => m.Map<LearningContentDto>(It.IsAny<LearningContent>()))
            .Returns(new LearningContentDto());

        // Act
        var result = await _service.UpdateLessonAsync(courseId, sectionId1, learningObjectId1, updateLessonDto, existingDto, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        result.Should().BeEquivalentTo(updateLessonDto, options => options
            .Excluding(dto => dto.ContentPath)
            .Excluding(dto => dto.FileName)
            .Excluding(dto => dto.FileSize)
            .Excluding(dto => dto.OrgId)
            .Excluding(dto => dto.DeleteFile));
        result.FileName.Should().NotBeEmpty().Equals(newFileName);
        result.ContentPath.Should().NotBeEmpty();
        result.FileSize.Should().NotBeEmpty().Equals("10");
        _mockBlobRepository.Verify(r => r.RemoveBlobAsync(containerName, contentPath, fileName), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateLessonAsync_WhenLessonTypeURLDeleteFileTrue_DoesNotDeleteBlobFile_Success()
    {
        // Arrange
        var containerName = "main";
        var orgId = new Random().Next(1, 100001).ToString();
        var courseId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var learningObjectId = Guid.NewGuid();
        var lessonName = new LocalizedStringDto { En = "Lesson.com" };
        var contentPath = "https://lesson.com";
        var fileName = "Click me to go to Lesson.com";
        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = sectionId,
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects =
                    [
                        new LessonDto
                        {
                            LearningObjectId = learningObjectId,
                            LearningObjectType = LearningObjectType.Lesson,
                            Name = lessonName,
                            LessonType = "url",
                            DurationMinutes = 10,
                            RequiredForCompletion = true,
                            RequiresAudio = false,
                            RequiresVideo = false,
                            OpensInNewTab = false,
                            ContentPath = contentPath,
                            FileName = fileName,
                            FileSize = "0"
                        }
                    ]
                }
            ]
        };

        var updateLessonDto = new UpdateLessonDto
        {
            LearningObjectId = learningObjectId,
            LearningObjectType = LearningObjectType.Lesson,
            Name = lessonName,
            LessonType = "url",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = contentPath,
            FileName = fileName,
            FileSize = "0",
            OrgId = orgId,
            DeleteFile = true
        };

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken))
            .ReturnsAsync(new LearningContent());
        _mockMapper
            .Setup(m => m.Map<LearningContent>(It.IsAny<LearningContentDto>()))
            .Returns(new LearningContent());
        _mockMapper
            .Setup(m => m.Map<LearningContentDto>(It.IsAny<LearningContent>()))
            .Returns(new LearningContentDto());

        // Act
        var result = await _service.UpdateLessonAsync(courseId, sectionId, learningObjectId, updateLessonDto, existingDto, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        result.Should().BeEquivalentTo(updateLessonDto, options => options
            .Excluding(dto => dto.OrgId)
            .Excluding(dto => dto.DeleteFile));
        _mockBlobRepository.Verify(r => r.GetFilePropertiesAsync(containerName, contentPath, fileName), Times.Never);
        _mockBlobRepository.Verify(r => r.RemoveBlobAsync(containerName, contentPath, fileName), Times.Never);
    }

    [Fact]
    public async Task UpdateLessonAsync_WhenDeleteFileTrue_DeletesBlobFile_Fails_LogsWarning()
    {
        // Arrange
        var containerName = "main";
        var orgId = new Random().Next(1, 100001).ToString();
        var courseId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var learningObjectId = Guid.NewGuid();
        var lessonName = new LocalizedStringDto { En = "Lesson to Delete" };
        var fileName = "lessonToDelete.mp4";
        var contentPath = $"{orgId}/{courseId}/{learningObjectId}/";
        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = sectionId,
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects =
                    [
                        new LessonDto
                        {
                            LearningObjectId = learningObjectId,
                            LearningObjectType = LearningObjectType.Lesson,
                            Name = lessonName,
                            LessonType = "file",
                            DurationMinutes = 10,
                            RequiredForCompletion = true,
                            RequiresAudio = false,
                            RequiresVideo = false,
                            OpensInNewTab = false,
                            ContentPath = containerName,
                            FileName = fileName,
                            FileSize = "10"
                        }
                    ]
                }
            ]
        };

        var updateLessonDto = new UpdateLessonDto
        {
            LearningObjectId = learningObjectId,
            LearningObjectType = LearningObjectType.Lesson,
            Name = lessonName,
            LessonType = "file",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = containerName,
            FileName = fileName,
            FileSize = "10",
            OrgId = orgId,
            DeleteFile = true
        };

        _mockBlobRepository.Setup(r => r.RemoveBlobAsync(containerName, contentPath, fileName))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken))
            .ReturnsAsync(new LearningContent());
        _mockMapper
            .Setup(m => m.Map<LearningContent>(It.IsAny<LearningContentDto>()))
            .Returns(new LearningContent());
        _mockMapper
            .Setup(m => m.Map<LearningContentDto>(It.IsAny<LearningContent>()))
            .Returns(new LearningContentDto());

        // Act
        var result = await _service.UpdateLessonAsync(courseId, sectionId, learningObjectId, updateLessonDto, existingDto, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        result.Should().BeEquivalentTo(updateLessonDto, options => options
            .Excluding(dto => dto.OrgId)
            .Excluding(dto => dto.DeleteFile));
        _mockBlobRepository.Verify(r => r.RemoveBlobAsync(containerName, contentPath, fileName), Times.Once);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Could not delete associated file for lesson")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
    }

    #endregion

    #region LessonPatchTests
    //commenting out for now - future ticket will target using UpdateLearningContentAsync for learning objects where these can be updated

    //[Fact]
    //public async Task UpdateLearningContentAsync_WhenLessonIsNull_ThrowsInvalidOperationException()
    //{
    //    var courseId = Guid.NewGuid();
    //    var sectionId = Guid.NewGuid();

    //    var existing = new LearningContent
    //    {
    //        CourseId = courseId,
    //        Sections = [
    //            new LearningContentSection { 
    //                SectionId = sectionId,
    //                Name = new LocalizedString { En = "Section 1" },
    //                LearningObjects = [] 
    //            }
    //        ]
    //    };

    //    var patchDto = new LearningContentPatchDto
    //    {
    //        LearningObjectUpdate = new LearningObjectUpdateDto
    //        {
    //            Action = PatchAction.Add,
    //            SectionId = sectionId,
    //            LearningObjectType = LearningObjectType.Lesson,
    //            Lesson = null
    //        }
    //    };

    //    _mockRepository.Setup(r => r.GetByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(existing);

    //    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
    //        _service.UpdateLearningContentAsync(courseId, sectionId, patchDto, existing, _cancellationToken));

    //    Assert.Equal("Lesson data is required", ex.Message);
    //}

    //[Fact]
    //public async Task UpdateLearningContentAsync_WhenLessonNameIsMissing_ThrowsInvalidOperationException()
    //{
    //    var courseId = Guid.NewGuid();
    //    var sectionId = Guid.NewGuid();

    //    var existing = new LearningContent
    //    {
    //        CourseId = courseId,
    //        Sections = [
    //            new LearningContentSection {
    //                SectionId = sectionId,
    //                Name = new LocalizedString { En = "Section 1" },
    //                LearningObjects = []
    //            }
    //        ]
    //    };

    //    var patchDto = new LearningContentPatchDto
    //    {
    //        LearningObjectUpdate = new LearningObjectUpdateDto
    //        {
    //            Action = PatchAction.Add,
    //            SectionId = sectionId,
    //            LearningObjectType = LearningObjectType.Lesson,
    //            Lesson = new LessonUpdateDto
    //            {
    //                Name = null,
    //                LessonType = "video",
    //                DurationMinutes = 5
    //            }
    //        }
    //    };

    //    _mockRepository.Setup(r => r.GetByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(existing);

    //    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
    //        _service.UpdateLearningContentAsync(courseId, sectionId, patchDto, existing, _cancellationToken));

    //    Assert.Equal("Lesson name is required.", ex.Message);
    //}

    //[Fact]
    //public async Task UpdateLearningContentAsync_WhenLessonTypeIsMissing_ThrowsInvalidOperationException()
    //{
    //    var courseId = Guid.NewGuid();
    //    var sectionId = Guid.NewGuid();

    //    var existing = new LearningContent
    //    {
    //        CourseId = courseId,
    //        Sections = [
    //            new LearningContentSection {
    //                SectionId = sectionId,
    //                Name = new LocalizedString { En = "Section 1" },
    //                LearningObjects = []
    //            }
    //        ]
    //    };

    //    var patchDto = new LearningContentPatchDto
    //    {
    //        LearningObjectUpdate = new LearningObjectUpdateDto
    //        {
    //            Action = PatchAction.Add,
    //            SectionId = sectionId,
    //            LearningObjectType = LearningObjectType.Lesson,
    //            Lesson = new LessonUpdateDto
    //            {
    //                Name = new LocalizedStringDto { En = "Lesson X" },
    //                LessonType = null,
    //                DurationMinutes = 10
    //            }
    //        }
    //    };

    //    _mockRepository.Setup(r => r.GetByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(existing);

    //    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
    //        _service.UpdateLearningContentAsync(courseId, sectionId, patchDto, existing, _cancellationToken));

    //    Assert.Equal("Lesson type is required.", ex.Message);
    //}

    //[Fact]
    //public async Task UpdateLearningContentAsync_WhenLessonDurationIsInvalid_ThrowsInvalidOperationException()
    //{
    //    var courseId = Guid.NewGuid();
    //    var sectionId = Guid.NewGuid();

    //    var existing = new LearningContent
    //    {
    //        CourseId = courseId,
    //        Sections = [
    //            new LearningContentSection {
    //                SectionId = sectionId,
    //                Name = new LocalizedString { En = "Section 1" },
    //                LearningObjects = []
    //            }
    //        ]
    //    };

    //    var patchDto = new LearningContentPatchDto
    //    {
    //        LearningObjectUpdate = new LearningObjectUpdateDto
    //        {
    //            Action = PatchAction.Add,
    //            SectionId = sectionId,
    //            LearningObjectType = LearningObjectType.Lesson,
    //            Lesson = new LessonUpdateDto
    //            {
    //                Name = new LocalizedStringDto { En = "Lesson X" },
    //                LessonType = "video",
    //                DurationMinutes = 0
    //            }
    //        }
    //    };

    //    _mockRepository.Setup(r => r.GetByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(existing);

    //    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
    //        _service.UpdateLearningContentAsync(courseId, sectionId, patchDto, existing, _cancellationToken));

    //    Assert.Equal("Lesson duration should be greater than 0.", ex.Message);
    //}

    //[Fact]
    //public async Task UpdateLearningContentAsync_WhenAddingNewLesson_AddsSuccessfully()
    //{
    //    var id = Guid.NewGuid();
    //    var courseId = Guid.NewGuid();
    //    var sectionId = Guid.NewGuid();
    //    var newLessonId = Guid.NewGuid();

    //    var existing = new LearningContent
    //    {
    //        Id = id,
    //        CourseId = courseId,
    //        Sections =
    //        [
    //            new LearningContentSection
    //        {
    //            SectionId = sectionId,
    //            Name = new LocalizedString { En = "Section 1" },
    //            LearningObjects = []
    //        }
    //        ]
    //    };

    //    var lessonUpdateDto = new LessonUpdateDto
    //    {
    //        LearningObjectId = newLessonId,
    //        LessonType = "text",
    //        Name = new LocalizedStringDto { En = "Lesson 1" },
    //        DurationMinutes = 60,
    //        RequiredForCompletion = true,
    //        RequiresAudio = false,
    //        RequiresVideo = false,
    //        OpensInNewTab = false,
    //        ContentPath = "https://blobstorage.com/lesson1"
    //    };

    //    var patchDto = new LearningContentPatchDto
    //    {
    //        LearningObjectUpdate = new LearningObjectUpdateDto
    //        {
    //            Action = PatchAction.Add,
    //            SectionId = sectionId,
    //            LearningObjectId = newLessonId,
    //            LearningObjectType = LearningObjectType.Lesson,
    //            Lesson = lessonUpdateDto
    //        }
    //    };

    //    var lesson = new Lesson
    //    {
    //        LearningObjectId = newLessonId,
    //        LessonType = "text",
    //        Name = new LocalizedString { En = "Lesson 1" },
    //        DurationMinutes = 60,
    //        RequiredForCompletion = true,
    //        RequiresAudio = false,
    //        RequiresVideo = false,
    //        OpensInNewTab = false,
    //        ContentPath = "https://blobstorage.com/lesson1"
    //    };

    //    var updated = new LearningContent
    //    {
    //        Id = id,
    //        CourseId = courseId,
    //        Sections =
    //        [
    //            new LearningContentSection
    //        {
    //            SectionId = sectionId,
    //            Name = new LocalizedString { En = "Section 1" },
    //            LearningObjects =
    //            [
    //                lesson
    //            ]
    //        }
    //        ]
    //    };

    //    var expectedDto = new LearningContentDto
    //    {
    //        Id = id,
    //        CourseId = courseId,
    //        Sections =
    //        [
    //            new LearningContentSectionDto
    //        {
    //            SectionId = sectionId,
    //            Name = new LocalizedStringDto { En = "Section 1" },
    //            LearningObjects =
    //            [
    //                new LessonDto
    //                {
    //                    LearningObjectId = newLessonId,
    //                    LessonType = "text",
    //                    Name = new LocalizedStringDto { En = "Lesson 1" },
    //                    DurationMinutes = 60,
    //                    RequiredForCompletion = true,
    //                    RequiresAudio = false,
    //                    RequiresVideo = false,
    //                    OpensInNewTab = false,
    //                    ContentPath = "https://blobstorage.com/lesson1"
    //                }
    //            ]
    //        }
    //        ]
    //    };

    //    _mockRepository.Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
    //        .ReturnsAsync(existing);

    //    _mockRepository.Setup(repo => repo.UpdateAsync(existing, _cancellationToken))
    //        .ReturnsAsync(updated);

    //    _mockMapper.Setup(m => m.Map<Lesson>(lessonUpdateDto))
    //        .Returns(lesson);

    //    _mockMapper.Setup(m => m.Map<LearningContentDto>(updated))
    //        .Returns(expectedDto);

    //    var result = await _service.UpdateLearningContentAsync(courseId, sectionId, patchDto, existing, _cancellationToken);

    //    Assert.NotNull(result);
    //    var returnedLesson = Assert.IsType<LessonDto>(result.Sections[0].LearningObjects[0]);
    //    Assert.Equal(newLessonId, returnedLesson.LearningObjectId);
    //    Assert.Equal("Lesson 1", returnedLesson.Name.En);

    //    _mockRepository.Verify(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken), Times.Once);
    //    _mockRepository.Verify(repo => repo.UpdateAsync(existing, _cancellationToken), Times.Once);
    //    _mockMapper.Verify(mapper => mapper.Map<LearningContentDto>(updated), Times.Once);
    //}

    //[Fact]
    //public async Task UpdateLearningContentAsync_WhenRemovingLessonWithEmptyId_ThrowsInvalidOperationException()
    //{
    //    var courseId = Guid.NewGuid();
    //    var sectionId = Guid.NewGuid();

    //    var existing = new LearningContent
    //    {
    //        CourseId = courseId,
    //        Sections =
    //        [
    //            new LearningContentSection
    //        {
    //            SectionId = sectionId,
    //            Name = new LocalizedString { En = "Section 1" },
    //            LearningObjects = []
    //        }
    //        ]
    //    };

    //    var patchDto = new LearningContentPatchDto
    //    {
    //        LearningObjectUpdate = new LearningObjectUpdateDto
    //        {
    //            Action = PatchAction.Remove,
    //            SectionId = sectionId,
    //            LearningObjectId = Guid.Empty,
    //            LearningObjectType = LearningObjectType.Lesson
    //        }
    //    };

    //    _mockRepository.Setup(r => r.GetByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(existing);

    //    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
    //        _service.UpdateLearningContentAsync(courseId, sectionId, patchDto, existing, _cancellationToken));

    //    Assert.Equal("LearningObjectId is required to remove a learning object.", ex.Message);
    //}

    //[Fact]
    //public async Task UpdateLearningContentAsync_WhenRemovingLessonNotInSection_ThrowsInvalidOperationException()
    //{
    //    var courseId = Guid.NewGuid();
    //    var sectionId = Guid.NewGuid();
    //    var missingLessonId = Guid.NewGuid();

    //    var existing = new LearningContent
    //    {
    //        CourseId = courseId,
    //        Sections =
    //        [
    //            new LearningContentSection
    //        {
    //            SectionId = sectionId,
    //            Name = new LocalizedString { En = "Section 1" },
    //            LearningObjects = []
    //        }
    //        ]
    //    };

    //    var patchDto = new LearningContentPatchDto
    //    {
    //        LearningObjectUpdate = new LearningObjectUpdateDto
    //        {
    //            Action = PatchAction.Remove,
    //            SectionId = sectionId,
    //            LearningObjectId = missingLessonId,
    //            LearningObjectType = LearningObjectType.Lesson
    //        }
    //    };

    //    _mockRepository.Setup(r => r.GetByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(existing);

    //    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
    //        _service.UpdateLearningContentAsync(courseId, sectionId, patchDto, existing, _cancellationToken));

    //    Assert.Contains("not found in section", ex.Message);
    //}

    //[Fact]
    //public async Task UpdateLearningContentAsync_WhenRemovingLesson_RemovesSuccessfully()
    //{
    //    var id = Guid.NewGuid();
    //    var courseId = Guid.NewGuid();
    //    var sectionId = Guid.NewGuid();
    //    var existingLessonId = Guid.NewGuid();

    //    var existing = new LearningContent
    //    {
    //        Id = id,
    //        CourseId = courseId,
    //        Sections =
    //        [
    //            new LearningContentSection
    //        {
    //            SectionId = sectionId,
    //            Name = new LocalizedString { En = "Section 1" },
    //            LearningObjects =
    //            [
    //                new Lesson
    //                {
    //                    LearningObjectId = existingLessonId,
    //                    LessonType = "text",
    //                    Name = new LocalizedString { En = "Lesson 1" },
    //                    DurationMinutes = 30,
    //                    RequiredForCompletion = true,
    //                    RequiresAudio = false,
    //                    RequiresVideo = false,
    //                    OpensInNewTab = false,
    //                    ContentPath = "https://blobstorage.com/lesson1"
    //                }
    //            ]
    //        }
    //        ]
    //    };

    //    var patchDto = new LearningContentPatchDto
    //    {
    //        LearningObjectUpdate = new LearningObjectUpdateDto
    //        {
    //            Action = PatchAction.Remove,
    //            SectionId = sectionId,
    //            LearningObjectId = existingLessonId,
    //            LearningObjectType = LearningObjectType.Lesson
    //        }
    //    };

    //    var updated = new LearningContent
    //    {
    //        Id = id,
    //        CourseId = courseId,
    //        Sections =
    //        [
    //            new LearningContentSection
    //        {
    //            SectionId = sectionId,
    //            Name = new LocalizedString { En = "Section 1" },
    //            LearningObjects = []
    //        }
    //        ]
    //    };

    //    var expectedDto = new LearningContentDto
    //    {
    //        Id = id,
    //        CourseId = courseId,
    //        Sections =
    //        [
    //            new LearningContentSectionDto
    //        {
    //            SectionId = sectionId,
    //            Name = new LocalizedStringDto { En = "Section 1" },
    //            LearningObjects = []
    //        }
    //        ]
    //    };

    //    _mockRepository.Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
    //        .ReturnsAsync(existing);

    //    _mockRepository.Setup(repo => repo.UpdateAsync(existing, _cancellationToken))
    //        .ReturnsAsync(updated);

    //    _mockMapper.Setup(m => m.Map<LearningContentDto>(updated))
    //        .Returns(expectedDto);

    //    var result = await _service.UpdateLearningContentAsync(courseId, sectionId, patchDto, existing, _cancellationToken);

    //    Assert.NotNull(result);
    //    Assert.Empty(result.Sections[0].LearningObjects);

    //    _mockRepository.Verify(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken), Times.Once);
    //    _mockRepository.Verify(repo => repo.UpdateAsync(existing, _cancellationToken), Times.Once);
    //    _mockMapper.Verify(mapper => mapper.Map<LearningContentDto>(updated), Times.Once);
    //}


    //[Fact]
    //public async Task UpdateLearningContentAsync_WhenReplacingLesson_ReplacesSuccessfully()
    //{
    //    var id = Guid.NewGuid();
    //    var courseId = Guid.NewGuid();
    //    var sectionId = Guid.NewGuid();
    //    var existingLessonId = Guid.NewGuid();

    //    var existing = new LearningContent
    //    {
    //        Id = id,
    //        CourseId = courseId,
    //        Sections =
    //        [
    //            new LearningContentSection
    //        {
    //            SectionId = sectionId,
    //            Name = new LocalizedString { En = "Section 1" },
    //            LearningObjects =
    //            [
    //                new Lesson
    //                {
    //                    LearningObjectId = existingLessonId,
    //                    LessonType = "text",
    //                    Name = new LocalizedString { En = "Lesson 1" },
    //                    DurationMinutes = 60,
    //                    RequiredForCompletion = true,
    //                    RequiresAudio = false,
    //                    RequiresVideo = false,
    //                    OpensInNewTab = false,
    //                    ContentPath = "https://blobstorage.com/original"
    //                }
    //            ]
    //        }
    //        ]
    //    };

    //    var patchDto = new LearningContentPatchDto
    //    {
    //        LearningObjectUpdate = new LearningObjectUpdateDto
    //        {
    //            Action = PatchAction.Replace,
    //            SectionId = sectionId,
    //            LearningObjectId = existingLessonId,
    //            LearningObjectType = LearningObjectType.Lesson,
    //            Lesson = new LessonUpdateDto
    //            {
    //                LessonType = "text",
    //                Name = new LocalizedStringDto { En = "Updated Lesson 1" },
    //                DurationMinutes = 90,
    //                RequiredForCompletion = true,
    //                RequiresAudio = false,
    //                RequiresVideo = false,
    //                OpensInNewTab = false,
    //                ContentPath = "https://blobstorage.com/updated"
    //            }
    //        }
    //    };

    //    var updated = new LearningContent
    //    {
    //        Id = id,
    //        CourseId = courseId,
    //        Sections =
    //        [
    //            new LearningContentSection
    //        {
    //            SectionId = sectionId,
    //            Name = new LocalizedString { En = "Section 1" },
    //            LearningObjects =
    //            [
    //                new Lesson
    //                {
    //                    LearningObjectId = existingLessonId,
    //                    LessonType = "text",
    //                    Name = new LocalizedString { En = "Updated Lesson 1" },
    //                    DurationMinutes = 90,
    //                    RequiredForCompletion = true,
    //                    RequiresAudio = false,
    //                    RequiresVideo = false,
    //                    OpensInNewTab = false,
    //                    ContentPath = "https://blobstorage.com/updated"
    //                }
    //            ]
    //        }
    //        ]
    //    };

    //    var expectedDto = new LearningContentDto
    //    {
    //        Id = id,
    //        CourseId = courseId,
    //        Sections =
    //        [
    //            new LearningContentSectionDto
    //        {
    //            SectionId = sectionId,
    //            Name = new LocalizedStringDto { En = "Section 1" },
    //            LearningObjects =
    //            [
    //                new LessonDto
    //                {
    //                    LearningObjectId = existingLessonId,
    //                    LessonType = "text",
    //                    Name = new LocalizedStringDto { En = "Updated Lesson 1" },
    //                    DurationMinutes = 90,
    //                    RequiredForCompletion = true,
    //                    RequiresAudio = false,
    //                    RequiresVideo = false,
    //                    OpensInNewTab = false,
    //                    ContentPath = "https://blobstorage.com/updated"
    //                }
    //            ]
    //        }
    //        ]
    //    };

    //    _mockRepository.Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
    //        .ReturnsAsync(existing);

    //    _mockRepository.Setup(repo => repo.UpdateAsync(existing, _cancellationToken))
    //        .ReturnsAsync(updated);

    //    _mockMapper.Setup(mapper => mapper.Map<LearningContentDto>(updated))
    //        .Returns(expectedDto);

    //    var result = await _service.UpdateLearningContentAsync(courseId, sectionId, patchDto, existing, _cancellationToken);

    //    Assert.NotNull(result);
    //    var updatedLesson = Assert.IsType<LessonDto>(result.Sections[0].LearningObjects[0]);
    //    Assert.Equal(existingLessonId, updatedLesson.LearningObjectId);
    //    Assert.Equal("Updated Lesson 1", updatedLesson.Name.En);
    //    Assert.Equal(90, updatedLesson.DurationMinutes);

    //    _mockRepository.Verify(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken), Times.Once);
    //    _mockRepository.Verify(repo => repo.UpdateAsync(existing, _cancellationToken), Times.Once);
    //    _mockMapper.Verify(mapper => mapper.Map<LearningContentDto>(updated), Times.Once);
    //}
    #endregion

    #region UploadLessonFileAsync Tests

    private static IFormFile CreateMockFormFile(string fileName = "test.mp4", string contentType = "video/mp4", long length = 5)
    {
        var mockFile = new Mock<IFormFile>();
        var content = new byte[length];
        var stream = new MemoryStream(content);

        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream target, CancellationToken ct) =>
            {
                stream.Position = 0;
                return stream.CopyToAsync(target, ct);
            });
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);

        return mockFile.Object;
    }

    [Fact]
    public async Task UploadLessonFileAsync_WithIntermediateChunk_StagesBlockOnly()
    {
        // Arrange
        var orgId = 123;
        var courseId = Guid.NewGuid();
        var learningObjectId = Guid.NewGuid();
        var uploadId = "upload-abc";
        var fileName = "test.pdf";
        var chunkIndex = 0;
        var totalChunks = 3;
        var expectedBlockId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{chunkIndex:D6}"));
        var mockFormFile = CreateMockFormFile(fileName, "application/pdf", 50);

        _mockBlobRepository
            .Setup(x => x.StageBlockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
            .ReturnsAsync(Mock.Of<Azure.Storage.Blobs.Models.BlockInfo>());

        BlockIdMemoryStore.Add(uploadId, chunkIndex, expectedBlockId, totalChunks);
        await _service.UploadLessonFileAsync(orgId, courseId, learningObjectId, mockFormFile, uploadId, chunkIndex, totalChunks, fileName, _cancellationToken);

        _mockBlobRepository.Verify(x => x.StageBlockAsync(
            AzureSdkClientConstants.MainContainer,
            $"{orgId}/{courseId}/{learningObjectId}",
            fileName,
            expectedBlockId,
            It.IsAny<MemoryStream>()), Times.Once);

        _mockBlobRepository.Verify(x => x.CommitBlockListAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task UploadLessonFileAsync_WithFinalChunk_CommitsBlockList()
    {
        // Arrange
        var orgId = 123;
        var courseId = Guid.NewGuid();
        var learningObjectId = Guid.NewGuid();
        var uploadId = "upload-final";
        var fileName = "final.pdf";
        var chunkIndex = 2;
        var totalChunks = 3;
        var expectedBlockId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{chunkIndex:D6}"));
        var mockFormFile = CreateMockFormFile(fileName, "application/pdf", 50);

        var expectedBlockList = new List<string>
        {
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"000000")),
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"000001")),
            expectedBlockId
        };

        BlockIdMemoryStore.Add(uploadId, 0, expectedBlockList[0], totalChunks);
        BlockIdMemoryStore.Add(uploadId, 1, expectedBlockList[1], totalChunks);

        _mockBlobRepository
            .Setup(x => x.StageBlockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
            .ReturnsAsync(Mock.Of<Azure.Storage.Blobs.Models.BlockInfo>());

        _mockBlobRepository
            .Setup(x => x.CommitBlockListAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .ReturnsAsync(Mock.Of<Azure.Storage.Blobs.Models.BlobContentInfo>());

        // Act
        await _service.UploadLessonFileAsync(orgId, courseId, learningObjectId, mockFormFile, uploadId, chunkIndex, totalChunks, fileName, _cancellationToken);

        // Assert
        _mockBlobRepository.Verify(x => x.StageBlockAsync(
            AzureSdkClientConstants.MainContainer,
            $"{orgId}/{courseId}/{learningObjectId}",
            fileName,
            expectedBlockId,
            It.IsAny<MemoryStream>()), Times.Once);

        _mockBlobRepository.Verify(x => x.CommitBlockListAsync(
            AzureSdkClientConstants.MainContainer,
            $"{orgId}/{courseId}/{learningObjectId}",
            fileName,
            It.Is<List<string>>(blocks => blocks.Count == 3)), Times.Once);
    }

    #endregion

    #region DeleteByCourseIdAsync Tests
    [Fact]
    public async Task DeleteByCourseIdAsync_WhenContentExists_DeletesContent()
    {
        var courseId = Guid.NewGuid();
        var existingContent = new LearningContent { Id = Guid.NewGuid(), CourseId = courseId };

        _mockRepository
            .Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(existingContent);

        _mockRepository
            .Setup(repo => repo.DeleteAsync(existingContent, _cancellationToken))
            .Returns(Task.CompletedTask);

        await _service.DeleteByCourseIdAsync(courseId, _cancellationToken);

        _mockRepository.Verify(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken), Times.Once);
        _mockRepository.Verify(repo => repo.DeleteAsync(existingContent, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task DeleteByCourseIdAsync_WhenContentDoesNotExist_DoesNothing()
    {
        var courseId = Guid.NewGuid();

        _mockRepository
            .Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync((LearningContent?)null);

        await _service.DeleteByCourseIdAsync(courseId, _cancellationToken);

        _mockRepository.Verify(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken), Times.Once);
        _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<LearningContent>(), _cancellationToken), Times.Never);
    }

    private void SetupMocksForLearningContentPatchFlow(Guid courseId, LearningContent existing, LearningContentDto existingDto, LearningContent updatedContent, LearningContentDto expectedDto)
    {
        _mockRepository
            .Setup(repo => repo.GetByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(existing);

        _mockMapper
            .Setup(m => m.Map<LearningContentDto>(existing))
            .Returns(existingDto);

        _mockMapper
            .Setup(m => m.Map<LearningContent>(existingDto))
            .Returns(updatedContent);

        _mockRepository
            .Setup(repo => repo.UpdateAsync(updatedContent, _cancellationToken))
            .ReturnsAsync(updatedContent);

        _mockMapper
            .Setup(m => m.Map<LearningContentDto>(updatedContent))
            .Returns(expectedDto);
    }

    #endregion

    #region Delete Lesson Tests

    [Fact]
    public async Task DeleteLessonAsync_WhenLessonTypeFile_DeletesBlobFileAndLesson_Success()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var learningObjectId = Guid.NewGuid();
        var orgId = 123;
        var fileName = "lesson.mp4";
        var contentPath = "main";

        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
            {
                SectionId = sectionId,
                Name = new LocalizedStringDto { En = "Section 1" },
                LearningObjects =
                [
                    new LessonDto
                    {
                        LearningObjectId = learningObjectId,
                        LearningObjectType = LearningObjectType.Lesson,
                        Name = new LocalizedStringDto { En = "Test Lesson" },
                        LessonType = "file",
                        ContentPath = contentPath,
                        FileName = fileName,
                        DurationMinutes = 10
                    }
                ]
            }
            ]
        };

        var expectedContentPath = $"{orgId}/{courseId}/{learningObjectId}/";

        _mockBlobRepository
            .Setup(r => r.RemoveBlobAsync(AzureSdkClientConstants.MainContainer, expectedContentPath, fileName))
            .ReturnsAsync(true);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken))
            .ReturnsAsync(new LearningContent());

        _mockMapper
            .Setup(m => m.Map<LearningContent>(It.IsAny<LearningContentDto>()))
            .Returns(new LearningContent());

        _mockMapper
            .Setup(m => m.Map<LearningContentDto>(It.IsAny<LearningContent>()))
            .Returns(new LearningContentDto());

        // Act
        await _service.DeleteLessonAsync(courseId, sectionId, learningObjectId, orgId, existingDto, _cancellationToken);

        // Assert
        _mockBlobRepository.Verify(r => r.RemoveBlobAsync(
            AzureSdkClientConstants.MainContainer,
            expectedContentPath,
            fileName), Times.Once);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task DeleteLessonAsync_WhenLessonTypeUrl_DoesNotDeleteBlobFile_Success()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var learningObjectId = Guid.NewGuid();
        var orgId = 123;

        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
            {
                SectionId = sectionId,
                Name = new LocalizedStringDto { En = "Section 1" },
                LearningObjects =
                [
                    new LessonDto
                    {
                        LearningObjectId = learningObjectId,
                        LearningObjectType = LearningObjectType.Lesson,
                        Name = new LocalizedStringDto { En = "Test Lesson" },
                        LessonType = "url",
                        ContentPath = "https://example.com",
                        FileName = "External Link",
                        DurationMinutes = 10
                    }
                ]
            }
            ]
        };

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken))
            .ReturnsAsync(new LearningContent());

        _mockMapper
            .Setup(m => m.Map<LearningContent>(It.IsAny<LearningContentDto>()))
            .Returns(new LearningContent());

        _mockMapper
            .Setup(m => m.Map<LearningContentDto>(It.IsAny<LearningContent>()))
            .Returns(new LearningContentDto());

        // Act
        await _service.DeleteLessonAsync(courseId, sectionId, learningObjectId, orgId, existingDto, _cancellationToken);

        // Assert
        _mockBlobRepository.Verify(r => r.RemoveBlobAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LearningContent>(), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task DeleteLessonAsync_WhenSectionNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var learningObjectId = Guid.NewGuid();
        var orgId = 123;

        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections = []
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteLessonAsync(courseId, sectionId, learningObjectId, orgId, existingDto, _cancellationToken));

        Assert.Equal($"Section {sectionId} not found for course {courseId}.", exception.Message);
    }

    [Fact]
    public async Task DeleteLessonAsync_WhenLessonNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var learningObjectId = Guid.NewGuid();
        var orgId = 123;

        var existingDto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = sectionId,
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects = []
                }
            ]
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteLessonAsync(courseId, sectionId, learningObjectId, orgId, existingDto, _cancellationToken));

        Assert.Equal($"Lesson {learningObjectId} not found in section {sectionId} for course {courseId}.", exception.Message);
    }

    #endregion
}