using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.App.Services;
using Relias.ContentLibraryService.Common.Enums;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Common.Interfaces;
using Relias.ContentLibraryService.Common.Resilience;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.App.Tests.Services;

public class CourseServiceTests
{
    private readonly Mock<ICourseRepository> _mockRepository = new();
    private readonly Mock<ILearningContentService> _mockLearningContentService = new();
    private readonly Mock<IFinalExamService> _mockFinalExamService = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ILogger<CourseService>> _mockLogger = new();
    private readonly Mock<IResilienceExecutor> _mockResilienceExecutor = new();
    private readonly Mock<IScheduledCourseRepository> _mockScheduledCourseRepository = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly CourseService _service;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private static readonly CreateCourseDto ValidNewCourse = new()
    {
        OrganizationId = 1,
        CourseName = "Valid Course",
        ContentCode = "VALID-COURSE-CODE"
    };

    private static readonly Guid ValidCreateCourseId = Guid.NewGuid();
    private static readonly Guid ValidCreateContentId = Guid.NewGuid();
    private static readonly Guid ValidLearningContentId = Guid.NewGuid();
    private static readonly string ValidCreateCreatedById = Guid.NewGuid().ToString();

    private static readonly Course ValidCreateCourseEntity = new()
    {
        CourseId = ValidCreateCourseId,
        ContentId = ValidCreateContentId,
        OrganizationId = 1,
        ContentCode = ValidNewCourse.ContentCode,
        Title = ValidNewCourse.CourseName,
        StatusId = 1,
        Created = DateTime.Now,
        CreatedBy = ValidCreateCreatedById.ToString()
    };

    private static readonly CourseDto ValidCourseDto = new()
    {
        CourseId = ValidCreateCourseId,
        ContentId = ValidCreateContentId,
        OrganizationId = 1,
        ContentCode = ValidNewCourse.ContentCode,
        Title = ValidNewCourse.CourseName,
        StatusId = 1,
        Created = DateTime.Now,
        CreatedBy = ValidCreateCreatedById,
        BriefDescription = string.Empty
    };

    private static readonly LearningContent learningContent = new()
    {
        Id = ValidLearningContentId,
        CourseId = ValidCreateCourseId,
        Sections =
            [
                new()
                {
                    SectionId = Guid.NewGuid(),
                    Name = new LocalizedString { En = "Section 1" },
                    LearningObjects = []
                }
            ]
    };

    public CourseServiceTests()
    {
        _service = new CourseService(
            _mockRepository.Object,
            _mockLearningContentService.Object,
            _mockFinalExamService.Object,
            _mockMapper.Object, _mockLogger.Object,
            _mockResilienceExecutor.Object,
            _mockScheduledCourseRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCourseExists_ReturnsMappedCourseDto()
    {
        var courseId = Guid.NewGuid();

        var course = new Course
        {
            CourseId = courseId,
            ContentId = Guid.NewGuid(),
            OrganizationId = 8,
            ContentCode = "C101",
            Title = "Test Course",
            BriefDescription = "Brief Test",
            LanguageIds = [Guid.NewGuid()],
            StatusId = 1,
            Created = DateTime.UtcNow,
            CreatedBy = "11111111-1111-1111-1111-111111111111"
        };

        var expectedDto = new CourseDto
        {
            CourseId = course.CourseId,
            ContentId = course.ContentId,
            OrganizationId = course.OrganizationId,
            ContentCode = course.ContentCode,
            Title = course.Title,
            BriefDescription = course.BriefDescription,
            LanguageIds = course.LanguageIds,
            StatusId = course.StatusId,
            Created = course.Created,
            CreatedBy = course.CreatedBy
        };

        _mockRepository
            .Setup(repo => repo.GetByIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(course);

        _mockMapper
            .Setup(mapper => mapper.Map<CourseDto>(course))
            .Returns(expectedDto);

        var result = await _service.GetByIdAsync(courseId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedDto.CourseId, result.CourseId);
        Assert.Equal(expectedDto.ContentId, result.ContentId);
        Assert.Equal(expectedDto.OrganizationId, result.OrganizationId);
        Assert.Equal(expectedDto.ContentCode, result.ContentCode);
        Assert.Equal(expectedDto.Title, result.Title);
        Assert.Equal(expectedDto.BriefDescription, result.BriefDescription);
        Assert.Equal(expectedDto.LanguageIds, result.LanguageIds);
        Assert.Equal(expectedDto.StatusId, result.StatusId);
        Assert.Equal(expectedDto.Created, result.Created);
        Assert.Equal(expectedDto.CreatedBy, result.CreatedBy);

        _mockRepository.Verify(repo => repo.GetByIdAsync(courseId, _cancellationToken), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<CourseDto>(course), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCourseDoesNotExist_ReturnsNull()
    {
        var courseId = Guid.NewGuid();

        _mockRepository
            .Setup(repo => repo.GetByIdAsync(courseId, _cancellationToken))
            .ReturnsAsync((Course?)null);

        var result = await _service.GetByIdAsync(courseId, _cancellationToken);

        Assert.Null(result);
        _mockRepository.Verify(repo => repo.GetByIdAsync(courseId, _cancellationToken), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<CourseDto>(null), Times.Once);
    }

    [Fact]
    public async Task GetCoursesAsync_WhenOrganizationIdExist_ReturnsListOfCourseDtos()
    {
        var organizationId = 8;
        var contentId = Guid.NewGuid();
        var courses = new List<Course>
        {
            new()
            {
                CourseId = Guid.NewGuid(),
                ContentId = contentId,
                OrganizationId = organizationId,
                ContentCode = "Code_1",
                Title = "Title_1",
                BriefDescription = "Brief_1",
                StatusId = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "11111111-1111-1111-1111-111111111111"
            },
            new()
            {
                CourseId = Guid.NewGuid(),
                ContentId = contentId,
                OrganizationId = organizationId,
                ContentCode = "Code_2",
                Title = "Title_2",
                BriefDescription = "Brief_2",
                StatusId = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "11111111-1111-1111-1111-111111111111"
            }
        };

        var expectedDtos = new List<CourseDto>
        {
            new()
            {
                CourseId = courses[0].CourseId,
                ContentId = courses[0].ContentId,
                OrganizationId = courses[0].OrganizationId,
                ContentCode = courses[0].ContentCode,
                Title = courses[0].Title,
                BriefDescription = courses[0].BriefDescription,
                StatusId = courses[0].StatusId,
                Created = courses[0].Created,
                CreatedBy = courses[0].CreatedBy
            },
            new()
            {
                CourseId = courses[1].CourseId,
                ContentId = courses[1].ContentId,
                OrganizationId = courses[1].OrganizationId,
                ContentCode = courses[1].ContentCode,
                Title = courses[1].Title,
                BriefDescription = courses[1].BriefDescription,
                StatusId = courses[1].StatusId,
                Created = courses[1].Created,
                CreatedBy = courses[1].CreatedBy
            }
        };


        _mockRepository
            .Setup(repo => repo.GetCoursesAsync(organizationId, _cancellationToken))
            .ReturnsAsync(courses);

        _mockMapper
            .Setup(mapper => mapper.Map<IEnumerable<CourseDto>?>(courses))
            .Returns(expectedDtos);

        var result = await _service.GetCoursesAsync(organizationId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedDtos[0].CourseId, result.FirstOrDefault()?.CourseId);
        Assert.Equal(expectedDtos[0].Title, result.FirstOrDefault()?.Title);
        Assert.Equal(expectedDtos[0].BriefDescription, result.FirstOrDefault()?.BriefDescription);
        Assert.Equal(expectedDtos[0].StatusId, result.FirstOrDefault()?.StatusId);

        _mockRepository.Verify(repo => repo.GetCoursesAsync(organizationId, _cancellationToken), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<IEnumerable<CourseDto>>(courses), Times.Once);
    }

    [Fact]
    public async Task GetCoursesAsync_WhenOrganizationIdNotExist_ReturnsEmptyList()
    {
        var organizationId = 8;
        var courses = new List<Course>();

        var expectedDtos = new List<CourseDto>();

        _mockRepository
            .Setup(repo => repo.GetCoursesAsync(organizationId, _cancellationToken))
            .ReturnsAsync(courses);

        _mockMapper
            .Setup(mapper => mapper.Map<List<CourseDto>?>(courses))
            .Returns(expectedDtos);

        var result = await _service.GetCoursesAsync(organizationId, _cancellationToken);
        Assert.NotNull(result);
        Assert.Empty(result);

        _mockRepository.Verify(repo => repo.GetCoursesAsync(organizationId, _cancellationToken), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<IEnumerable<CourseDto>>(courses), Times.Once);
    }

    [Fact]
    public async Task CreateCourseAsync_WhenValidNewCourseProvided_ReturnsCourseDto()
    {
        _mockRepository
            .Setup(cr => cr.CreateCourseAsync(ValidNewCourse, _cancellationToken))
            .ReturnsAsync(ValidCreateCourseEntity);

        _mockMapper
            .Setup(mapper => mapper.Map<CourseDto>(ValidCreateCourseEntity))
            .Returns(ValidCourseDto);

        _mockResilienceExecutor
            .Setup(r => r.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), _cancellationToken))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((func, token) => func(token));

        var result = await _service.CreateCourseAsync(ValidNewCourse, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(ValidCreateCourseId, result.CourseId);
        Assert.Equal(ValidCreateContentId, result.ContentId);

        _mockRepository.Verify(cr => cr.CreateCourseAsync(ValidNewCourse, _cancellationToken), Times.Once);
        _mockResilienceExecutor.Verify(p => p.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), _cancellationToken), Times.Once);
        _mockLearningContentService.Verify(lc => lc.InitializeByCourseIdAsync(ValidCreateCourseId, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ContentIdWillUpdate()
    {
        var contentId = Guid.NewGuid();
        var organizationId = 8;
        var courseId = Guid.NewGuid();
        var languageId = Guid.NewGuid();

        var updateCourseDto = new UpdateCourseDto
        {
            Title = "Updated Title",
            ContentId = contentId,
            OrganizationId = organizationId,
            ContentCode = "UPDATED-CODE",
            BriefDescription = "Updated Brief Description",
            Description = "Updated Description",
            LanguageIds = [languageId]
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            ContentId = Guid.NewGuid(),
            Title = "Old Title",
            OrganizationId = organizationId,
            ContentCode = "OLD-CODE",
            BriefDescription = "Old Brief",
            Description = "Old Description",
            LanguageIds = [],
            Created = DateTime.UtcNow,
            CreatedBy = "11111111-1111-1111-1111-111111111111"
        };

        var expectedCourseDto = new CourseDto
        {
            CourseId = courseId,
            ContentId = contentId,
            Title = "Updated Title",
            ContentCode = "UPDATED-CODE",
            BriefDescription = "Updated Brief Description",
            Description = "Updated Description",
            OrganizationId = organizationId,
            LanguageIds = [languageId],
            Created = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(existingCourse);

        _mockRepository
            .Setup(r => r.FindByContentCodeAsync(updateCourseDto.ContentCode, existingCourse.CourseId, _cancellationToken))
            .ReturnsAsync((Course?)null);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Course>(), _cancellationToken))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(m => m.Map<CourseDto>(existingCourse))
            .Returns(expectedCourseDto);

        var result = await _service.UpdateAsync(courseId, updateCourseDto, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(contentId, result.ContentId);
        Assert.Equal(courseId, result.CourseId);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("UPDATED-CODE", result.ContentCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenCourseDoesNotExist_ThrowsNotFoundException()
    {
        var courseId = Guid.NewGuid();
        var updateCourseDto = new UpdateCourseDto
        {
            Title = "Test",
            ContentCode = "C123",
            ContentId = null,
            OrganizationId = 8,
            Description = "Description",
            BriefDescription = "Brief",
            LanguageIds = [Guid.NewGuid()]
        };

        _mockRepository.Setup(r => r.GetByIdAsync(courseId, _cancellationToken))
            .ReturnsAsync((Course?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(courseId, updateCourseDto, _cancellationToken));
    }

    [Fact]
    public async Task UpdateAsync_WhenDuplicateContentCodeExists_ThrowsBadRequestException()
    {
        var courseId = Guid.NewGuid();
        var existingCourse = new Course { CourseId = courseId, ContentCode = "C101", Created = DateTime.UtcNow };
        var duplicateCourse = new Course { CourseId = Guid.NewGuid(), ContentCode = "C102", Created = DateTime.UtcNow };

        var updateDto = new UpdateCourseDto
        {
            ContentCode = duplicateCourse.ContentCode,
            Title = "Updated",
            OrganizationId = 1,
            ContentId = null,
            LanguageIds = [Guid.NewGuid()]
        };

        _mockRepository.Setup(r => r.GetByIdAsync(courseId, _cancellationToken)).ReturnsAsync(existingCourse);
        _mockRepository.Setup(r => r.FindByContentCodeAsync(updateDto.ContentCode, courseId, _cancellationToken)).ReturnsAsync(duplicateCourse);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateAsync(courseId, updateDto, _cancellationToken));
    }

    [Fact]
    public async Task ArchiveAsync_WhenCalled_ThrowsNotImplementedException()
    {
        var courseId = Guid.NewGuid();
        await Assert.ThrowsAsync<NotImplementedException>(() => _service.ArchiveAsync(courseId, _cancellationToken));
    }

    [Fact]
    public async Task DeleteAsync_WhenCourseIsValid_DeletesCourseAndContent()
    {
        var courseId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var course = new Course
        {
            CourseId = courseId,
            ContentId = contentId,
            StatusId = 1,
            Created = DateTime.UtcNow
        };

        var content = new Domain.Content.Content
        {
            ContentId = contentId
        };

        _mockRepository.Setup(r => r.GetByIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(course);

        _mockRepository.Setup(r => r.GetContentByCourseAsync(contentId, _cancellationToken))
            .ReturnsAsync(content);

        _mockRepository.Setup(r => r.DeleteAsync(course, content, _cancellationToken))
            .Returns(Task.CompletedTask);

        _mockResilienceExecutor
            .Setup(r => r.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), _cancellationToken))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((func, token) => func(token));

        await _service.DeleteAsync(courseId, _cancellationToken);
        _mockRepository.Verify(r => r.DeleteAsync(course, content, _cancellationToken), Times.Once);
        _mockLearningContentService.Verify(lc => lc.DeleteByCourseIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFinalExamService.Verify(fe => fe.DeleteFinalExamAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task DeleteAsync_WhenCourseNotFound_ThrowsNotFoundException()
    {
        var courseId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAsync(courseId, _cancellationToken))
            .ReturnsAsync((Course?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteAsync(courseId, _cancellationToken));
    }


    [Fact]
    public async Task DeleteAsync_WhenCourseIsNotDraft_ThrowsValidationException()
    {
        var courseId = Guid.NewGuid();
        var course = new Course
        {
            CourseId = courseId,
            StatusId = 2,
            Created = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(course);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.DeleteAsync(courseId, _cancellationToken));
    }

    [Fact]
    public async Task PublishAsync_WhenPublishingImmediately_UpdatesCourseStatusAndPublishesNow()
    {
        var courseId = Guid.NewGuid();
        var publishBy = Guid.NewGuid().ToString();
        var course = new Course
        {
            CourseId = courseId,
            StatusId = (int)StatusIdEnums.Draft,
            Created = DateTime.UtcNow
        };

        var dto = new PublishCourseDto
        {
            StatusId = (int)StatusIdEnums.Published,
            PublishBy = publishBy
        };

        _mockRepository.Setup(r => r.GetByIdAsync(courseId, _cancellationToken)).ReturnsAsync(course);
        _mockScheduledCourseRepository.Setup(r => r.DeleteByCourseIdAndStatusIdAsync(courseId, (int)StatusIdEnums.ScheduledForPublish, _cancellationToken)).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Course>(), _cancellationToken)).Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), _cancellationToken))
            .Returns<Func<Task>, CancellationToken>((func, token) => func());

        await _service.PublishAsync(courseId, dto, _cancellationToken);

        _mockScheduledCourseRepository.Verify(r => r.DeleteByCourseIdAndStatusIdAsync(courseId, (int)StatusIdEnums.ScheduledForPublish, _cancellationToken), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Course>(c => c.StatusId == (int)StatusIdEnums.Published && c.PublishBy == publishBy && c.PublishDate != null), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenSchedulingPublish_SchedulesAndUpdatesStatus()
    {
        var courseId = Guid.NewGuid();
        var publishDate = DateTime.UtcNow.AddDays(1);
        var publishBy = Guid.NewGuid().ToString();

        var course = new Course
        {
            CourseId = courseId,
            StatusId = (int)StatusIdEnums.Draft,
            Created = DateTime.UtcNow
        };

        var dto = new PublishCourseDto
        {
            StatusId = (int)StatusIdEnums.ScheduledForPublish,
            PublishBy = publishBy,
            PublishDate = publishDate
        };

        _mockRepository.Setup(r => r.GetByIdAsync(courseId, _cancellationToken)).ReturnsAsync(course);
        _mockScheduledCourseRepository.Setup(r => r.DeleteByCourseIdAndStatusIdAsync(courseId, (int)StatusIdEnums.ScheduledForPublish, _cancellationToken)).Returns(Task.CompletedTask);
        _mockScheduledCourseRepository.Setup(r => r.CreateAsync(It.IsAny<ScheduledCourse>(), _cancellationToken)).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Course>(), _cancellationToken)).Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), _cancellationToken))
            .Returns<Func<Task>, CancellationToken>((func, token) => func());

        await _service.PublishAsync(courseId, dto, _cancellationToken);

        _mockScheduledCourseRepository.Verify(r => r.CreateAsync(It.Is<ScheduledCourse>(s => s.CourseId == courseId && s.ScheduledDate == publishDate && s.ScheduledBy == publishBy), _cancellationToken), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Course>(c => c.StatusId == (int)StatusIdEnums.ScheduledForPublish && c.PublishDate == null), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenSchedulingWithoutDate_ThrowsInvalidOperationException()
    {
        var courseId = Guid.NewGuid();
        var publishBy = Guid.NewGuid().ToString();

        var course = new Course
        {
            CourseId = courseId,
            StatusId = (int)StatusIdEnums.Draft,
            Created = DateTime.UtcNow
        };

        var dto = new PublishCourseDto
        {
            StatusId = (int)StatusIdEnums.ScheduledForPublish,
            PublishBy = publishBy,
            PublishDate = null
        };

        _mockRepository.Setup(r => r.GetByIdAsync(courseId, _cancellationToken)).ReturnsAsync(course);

        _mockUnitOfWork
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), _cancellationToken))
            .Returns<Func<Task>, CancellationToken>((func, token) => func());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.PublishAsync(courseId, dto, _cancellationToken));
    }

    [Fact]
    public async Task PublishAsync_WhenCourseIsAlreadyPublished_ThrowsInvalidOperationException()
    {
        var courseId = Guid.NewGuid();
        var dto = new PublishCourseDto
        {
            StatusId = (int)StatusIdEnums.Published,
            PublishBy = Guid.NewGuid().ToString()
        };

        var course = new Course
        {
            CourseId = courseId,
            StatusId = (int)StatusIdEnums.Published,
            Created = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(courseId, _cancellationToken)).ReturnsAsync(course);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.PublishAsync(courseId, dto, _cancellationToken));
    }

    [Fact]
    public async Task PublishAsync_WhenCourseNotFound_ThrowsNotFoundException()
    {
        var courseId = Guid.NewGuid();
        var dto = new PublishCourseDto
        {
            StatusId = (int)StatusIdEnums.Published,
            PublishBy = Guid.NewGuid().ToString()
        };

        _mockRepository.Setup(r => r.GetByIdAsync(courseId, _cancellationToken)).ReturnsAsync((Course?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.PublishAsync(courseId, dto, _cancellationToken));
    }
}
