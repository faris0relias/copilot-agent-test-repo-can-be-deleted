using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.App.Features.Course.Enums;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.App.Services;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Common.Services;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.FinalExam;
using Relias.ContentLibraryService.Infra.Repositories;


namespace Relias.ContentLibraryService.App.Tests.Services;

public class FinalExamServiceTests
{
    private readonly Mock<IFinalExamRepository> _mockRepository = new();
    private readonly Mock<ICourseRepository> _mockCourseRepository = new();
    private readonly Mock<ILanguageContextService> _languageContextService = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ILogger<FinalExamService>> _mockLogger = new();
    private readonly FinalExamService _service;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService = new();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public FinalExamServiceTests()
    {
        _service = new FinalExamService(
            _mockRepository.Object,
            _mockLogger.Object,
            _mockCourseRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _languageContextService.Object
        );
    }

    [Fact]
    public async Task GetFinalExamByCourseIdAsync_WhenFinalExamExists_ReturnsMappedFinalExamDto()
    {
        var duration = new Duration
        {
            Hours = 1,
            Minutes = 30
        };
        var lang = "en";
        var finalExam = new FinalExam
        {
            Id = "49d7806e-4477-4ed9-ae88-34054259a471",
            CourseId = Guid.NewGuid(),
            MinimumPercentageToPass = 70,
            QuestionsDisplayedPerExam = 10,
            Duration = duration,
            Created = DateTime.UtcNow,
            CreatedBy = "11111111-1111-1111-1111-111111111111"
        };

        var expectedDto = new FinalExamDto
        {
            MinimumPercentageToPass = finalExam.MinimumPercentageToPass,
            QuestionsDisplayedPerExam = finalExam.QuestionsDisplayedPerExam,
            Duration = duration
        };

        _mockRepository
            .Setup(repo => repo.GetFinalExamByCourseIdAsync(finalExam.CourseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper
        .Setup(mapper => mapper.Map<FinalExamDto>(finalExam))
            .Returns(expectedDto);

        var result = await _service.GetFinalExamByCourseIdAsync(finalExam.CourseId, lang, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedDto.MinimumPercentageToPass, result.MinimumPercentageToPass);
        Assert.Equal(expectedDto.QuestionsDisplayedPerExam, result.QuestionsDisplayedPerExam);
        Assert.Equal(expectedDto.Duration.Hours, result.Duration!.Hours);
        Assert.Equal(expectedDto.Duration.Minutes, result.Duration!.Minutes);

        _mockRepository.Verify(repo => repo.GetFinalExamByCourseIdAsync(finalExam.CourseId, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetLearnerFinalExamSttingByCourseIdAsync_WhenFinalExamExists_ReturnsMappedFinalExamSttingsDto()
    {
        var duration = new Duration
        {
            Hours = 1,
            Minutes = 30
        };
        
        var finalExam = new FinalExam
        {
            Id = "49d7806e-4477-4ed9-ae88-34054259a471",
            CourseId = Guid.NewGuid(),
            MinimumPercentageToPass = 70,
            QuestionsDisplayedPerExam = 10,
            Duration = duration,
            Created = DateTime.UtcNow,
            CreatedBy = "11111111-1111-1111-1111-111111111111"
        };

        var expectedDto = new FinalExamSettingDto
        {
            MinimumPercentageToPass = (int)finalExam.MinimumPercentageToPass,
            QuestionsDisplayedPerExam = (int)finalExam.QuestionsDisplayedPerExam,
            Duration = duration
        };

        _mockRepository
            .Setup(repo => repo.GetFinalExamSettingsAsync(finalExam.CourseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper
        .Setup(mapper => mapper.Map<FinalExamSettingDto>(finalExam))
            .Returns(expectedDto);

        var result = await _service.GetFinalExamSettingsAsync(finalExam.CourseId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedDto.MinimumPercentageToPass, result.MinimumPercentageToPass);
        Assert.Equal(expectedDto.QuestionsDisplayedPerExam, result.QuestionsDisplayedPerExam);
        Assert.Equal(expectedDto.Duration.Hours, result.Duration!.Hours);
        Assert.Equal(expectedDto.Duration.Minutes, result.Duration!.Minutes);

        _mockRepository.Verify(repo => repo.GetFinalExamSettingsAsync(finalExam.CourseId, _cancellationToken), Times.Once);
    }

    
    [Fact]
    public async Task GetFinalExamByCourseIdAsync_WhenFinalExamWithQuestionsExists_ReturnsMappedFinalExamDto()
    {
        var lang = "en";
        var duration = new Duration
        {
            Hours = 1,
            Minutes = 30
        };

        var questionLocalizedString = new LocalizedString
        {
            En = "What is the main purpose of supervised learning?"
        };

        var optionsLocalizedString = new LocalizedString
        {
            En = "To find hidden patterns in unlabeled data"
        };

        var responseFeedbacksLocalizedString = new LocalizedString
        {
            En = "To find hidden patterns in unlabeled data"
        };

        var questionOptions = new List<FinalExamQuestionOptions>
        {
            new FinalExamQuestionOptions
            {
                 OptionId = Guid.NewGuid(),
                 OptionText = optionsLocalizedString,
                 IsCorrect = true,
                 ResponseFeedback= responseFeedbacksLocalizedString
            }
        };

        var questions = new List<FinalExamQuestion>
        {
            new FinalExamQuestion
            {
                QuestionId = Guid.NewGuid(),
                QuestionType = QuestionType.SingleSelect,
                QuestionText = questionLocalizedString,
                QuestionOptions = questionOptions
            }
        };

        var optionTextDto = new LocalizedStringDto
        {
            En = "To find hidden patterns in unlabeled data"
        };

        var responseFeedbackDto = new LocalizedStringDto
        {
            En = "To find hidden patterns in unlabeled data"
        };

        var questionsOptionsDto = new List<FinalExamQuestionOptionsDto>
        {
            new FinalExamQuestionOptionsDto
            {
                 OptionId = Guid.NewGuid(),
                 OptionText = optionTextDto,
                 IsCorrect = true,
                 ResponseFeedback= responseFeedbackDto
            }
        };

        var questionTextDto = new LocalizedStringDto
        {
            En = "What is the main purpose of supervised learning?"
        };

        var questionsDto = new List<FinalExamQuestionDto>
        {
            new FinalExamQuestionDto
            {
                QuestionId = Guid.NewGuid(),
                QuestionType = QuestionType.SingleSelect,
                QuestionText = questionTextDto,
                QuestionOptions = questionsOptionsDto
            }
        };




        var finalExam = new FinalExam
        {
            Id = "49d7806e-4477-4ed9-ae88-34054259a471",
            CourseId = Guid.NewGuid(),
            MinimumPercentageToPass = 70,
            QuestionsDisplayedPerExam = 10,
            Duration = duration,
            Created = DateTime.UtcNow,
            CreatedBy = "11111111-1111-1111-1111-111111111111",
            FinalExamQuestions = questions
        };

        var expectedDto = new FinalExamDto
        {
            MinimumPercentageToPass = finalExam.MinimumPercentageToPass,
            QuestionsDisplayedPerExam = finalExam.QuestionsDisplayedPerExam,
            Duration = duration,
            FinalExamQuestions = questionsDto
        };

        _mockRepository
            .Setup(repo => repo.GetFinalExamByCourseIdAsync(finalExam.CourseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper
            .Setup(mapper => mapper.Map<FinalExamDto>(finalExam))
            .Returns(expectedDto);

        var result = await _service.GetFinalExamByCourseIdAsync(finalExam.CourseId, lang, _cancellationToken);

        Assert.NotNull(result);
        Assert.NotNull(result.FinalExamQuestions);
        Assert.NotEmpty(result.FinalExamQuestions);

        var question = result.FinalExamQuestions[0];
        Assert.NotNull(question.QuestionOptions);
        Assert.NotEmpty(question.QuestionOptions);

        Assert.Equal(expectedDto.FinalExamQuestions[0].QuestionText.En, question.QuestionText.En);
        Assert.Equal(expectedDto?.FinalExamQuestions?[0].QuestionOptions?[0].OptionText!.En, question.QuestionOptions?[0].OptionText!.En);

    }

    [Fact]
    public async Task GetFinalExamByCourseIdAsync_WhenFinalExamDoesNotExist_ReturnsNull()
    {
        var courseId = Guid.NewGuid();
        var lang = "en";
        _mockRepository
            .Setup(repo => repo.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync((FinalExam?)null);

        var result = await _service.GetFinalExamByCourseIdAsync(courseId, lang, _cancellationToken);

        Assert.Null(result);
        _mockRepository.Verify(repo => repo.GetFinalExamByCourseIdAsync(courseId, _cancellationToken), Times.Once);
    }


    [Fact]
    public async Task GetLearnerFinalExamSettingsByCourseIdAsync_WhenFinalExamDoesNotExist_ReturnsNull()
    {
        var courseId = Guid.NewGuid();
      
        _mockRepository
            .Setup(repo => repo.GetFinalExamSettingsAsync(courseId, _cancellationToken))
            .ReturnsAsync((FinalExam?)null);

        var result = await _service.GetFinalExamSettingsAsync(courseId, _cancellationToken);

        Assert.Null(result);
        _mockRepository.Verify(repo => repo.GetFinalExamSettingsAsync(courseId, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateFinalExamAsync_WhenFinalExamDoesNotExist_ReturnsMappedFinalExamDto()
    {
        var courseId = Guid.NewGuid();
        int orgId = 8;
        var userId = Guid.NewGuid().ToString();

        Course currentCourse = new()
        {
            CourseId = courseId,
            OrganizationId = orgId,
            Created = DateTime.UtcNow,
        };
        FinalExam finalExam = new()
        {
            CourseId = courseId,
            MinimumPercentageToPass = null,
            QuestionsDisplayedPerExam = null,
            Duration = null,
            Created = DateTime.UtcNow,
        };
        FinalExamDto createdFinalExam = new()
        {
            MinimumPercentageToPass = null,
            QuestionsDisplayedPerExam = null,
            Duration = null
        };

        _mockRepository
            .Setup(repo => repo.CreateFinalExamAsync(courseId, userId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockCourseRepository
            .Setup(repo => repo.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken))
            .ReturnsAsync(currentCourse);

        _mockCurrentUserService.Setup(c => c.OrganizationId).Returns(orgId.ToString);

        _mockMapper
            .Setup(mapper => mapper.Map<FinalExamDto>(finalExam))
            .Returns(createdFinalExam);

        _mockCourseRepository
            .Setup(repo => repo.TriggerCourseContentUpdateAsync(currentCourse.CourseId, _cancellationToken))
            .Returns(Task.CompletedTask)
            .Callback(() =>
            {
                _mockCourseRepository.Object.UpdateAsync(currentCourse, _cancellationToken);
            });

        _mockCurrentUserService.Setup(c => c.UserId).Returns(userId);

        var result = await _service.CreateFinalExamAsync(courseId, orgId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(createdFinalExam.MinimumPercentageToPass, result.MinimumPercentageToPass);

        _mockRepository.Verify(repo => repo.GetFinalExamByCourseIdAsync(courseId, _cancellationToken), Times.Once);
        _mockCourseRepository.Verify(repo => repo.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken), Times.Once);
        _mockRepository.Verify(repo => repo.CreateFinalExamAsync(courseId, userId, _cancellationToken), Times.Once);

        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(currentCourse.CourseId, _cancellationToken), Times.Once);
        _mockCourseRepository.Verify(repo => repo.UpdateAsync(currentCourse, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateFinalExamAsync_WhenFinalExamAlreadyExists_ThrowsBadRequest()
    {
        var courseId = Guid.NewGuid();
        int orgId = 8;
        var userId = Guid.NewGuid().ToString();

        Course currentCourse = new()
        {
            CourseId = courseId,
            OrganizationId = orgId,
            Created = DateTime.UtcNow,
        };
        FinalExam finalExam = new()
        {
            CourseId = courseId,
            Created = DateTime.UtcNow,
        };
        var existingExam = new FinalExam
        {
            CourseId = courseId,
            Created = DateTime.UtcNow
        };
        FinalExamDto createdFinalExam = new()
        {
            MinimumPercentageToPass = null,
            QuestionsDisplayedPerExam = null,
            Duration = null
        };

        _mockCurrentUserService.Setup(c => c.OrganizationId).Returns(orgId.ToString);

        _mockRepository
            .Setup(repo => repo.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(existingExam);

        _mockCourseRepository
            .Setup(repo => repo.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken))
            .ReturnsAsync(currentCourse);

        _mockMapper.Setup(mapper => mapper.Map<FinalExamDto>(finalExam)).Returns(createdFinalExam);

        var result = await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateFinalExamAsync(courseId, orgId, _cancellationToken));

        Assert.Equal($"Final exam with id {courseId} already exists!", result.Message);

        _mockCourseRepository.Verify(repo => repo.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken), Times.Once);
        _mockRepository.Verify(repo => repo.GetFinalExamByCourseIdAsync(courseId, _cancellationToken), Times.Once);
        _mockRepository.Verify(repo => repo.CreateFinalExamAsync(It.IsAny<Guid>(), userId, It.IsAny<CancellationToken>()), Times.Never);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(currentCourse.CourseId, _cancellationToken), Times.Never);
        _mockCourseRepository.Verify(repo => repo.UpdateAsync(currentCourse, _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task CreateFinalExamAsync_WhenCourseDoesNotExist_ThrowsBadRequest()
    {
        var courseId = Guid.NewGuid();
        var orgId = 8;
        var userId = Guid.NewGuid().ToString();

        _mockCourseRepository
            .Setup(repo => repo.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken))
            .ReturnsAsync((Course?)null);

        var result = await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateFinalExamAsync(courseId, orgId, _cancellationToken));

        Assert.Equal($"You can't add final exam. Course with id {courseId} does not exist", result.Message);
        _mockCourseRepository.Verify(repo => repo.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken), Times.Once);
        _mockRepository.Verify(r => r.GetFinalExamByCourseIdAsync(It.IsAny<Guid>(), _cancellationToken), Times.Never);
        _mockRepository.Verify(r => r.CreateFinalExamAsync(It.IsAny<Guid>(), userId, _cancellationToken), Times.Never);
        _mockMapper.Verify(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()), Times.Never);

        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task UpdateFinalExamAsync_WhenValid_ReturnsUpdatedDto()
    {
        var courseId = Guid.NewGuid();
        var orgId = 8;
        string userId = Guid.NewGuid().ToString();
        var currentCourse = new Course
        {
            CourseId = courseId,
            OrganizationId = orgId,
            StatusId = 1,
            Created = DateTime.UtcNow,
        };
        var existingExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
        };
        var updateDto = new Dictionary<string, dynamic> { { "MinimumPercentageToPass", 80 } };
        var updatedFinalExam = new FinalExam
        {
            CourseId = courseId,
            MinimumPercentageToPass = updateDto["MinimumPercentageToPass"],
            Created = existingExam.Created,
        };
        var expectedDto = new FinalExamDto
        {
            MinimumPercentageToPass = updatedFinalExam.MinimumPercentageToPass,
            QuestionsDisplayedPerExam = existingExam.QuestionsDisplayedPerExam,
        };

        _mockRepository
            .Setup(r => r.UpdateFinalExamAsync(courseId, userId, It.IsAny<Dictionary<string, dynamic>>(), _cancellationToken))
            .ReturnsAsync(updatedFinalExam);

        _mockCourseRepository
            .Setup(repo => repo.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken))
            .ReturnsAsync(currentCourse);

        _mockMapper
            .Setup(m => m.Map<FinalExamDto>(updatedFinalExam))
            .Returns(expectedDto);

        _mockCourseRepository
            .Setup(repo => repo.TriggerCourseContentUpdateAsync(currentCourse.CourseId, _cancellationToken))
            .Returns(Task.CompletedTask)
            .Callback(() =>
            {
                _mockCourseRepository.Object.UpdateAsync(currentCourse, _cancellationToken);
            });

        _mockCurrentUserService.Setup(c => c.UserId).Returns(userId);

        var result = await _service.UpdateFinalExamAsync(courseId, orgId, updateDto, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedDto.MinimumPercentageToPass, result.MinimumPercentageToPass);
        Assert.Equal(expectedDto.QuestionsDisplayedPerExam, result.QuestionsDisplayedPerExam);

        _mockCourseRepository.Verify(r => r.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken), Times.Once);
        _mockRepository.Verify(r => r.UpdateFinalExamAsync(courseId, userId, It.IsAny<Dictionary<string, dynamic>>(), _cancellationToken), Times.Once);
        _mockMapper.Verify(m => m.Map<FinalExamDto>(updatedFinalExam), Times.Once);
        _mockMapper.Verify(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()), Times.Once);

        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Once);
        _mockCourseRepository.Verify(repo => repo.UpdateAsync(currentCourse, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateFinalExamAsync_WhenCourseNotInDraft_ThrowsBadRequest()
    {
        var courseId = Guid.NewGuid();
        var orgId = 8;
        var notInDraftCourse = new Course
        {
            CourseId = courseId,
            OrganizationId = orgId,
            StatusId = 2,
            Created = DateTime.UtcNow
        };

        _mockCurrentUserService
            .Setup(s => s.OrganizationId)
            .Returns(orgId.ToString());

        _mockCourseRepository
            .Setup(repo => repo.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken))
            .ReturnsAsync(notInDraftCourse);

        var updateDto = new Dictionary<string, dynamic> { { "MinimumPercentageToPass", 75 } };
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.UpdateFinalExamAsync(courseId, orgId, updateDto, _cancellationToken));

        Assert.Equal($"Course with id {courseId} not in draft status!", ex.Message);
        _mockCourseRepository.Verify(r => r.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken), Times.Once);
        _mockRepository.Verify(r => r.GetFinalExamByCourseIdAsync(It.IsAny<Guid>(), _cancellationToken), Times.Never);

        _mockMapper.Verify(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()), Times.Never);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Never);
    }
    [Fact]
    public async Task UpdateFinalExamAsync_WhenFinalExamDoesNotExist_ThrowsBadRequest()
    {
        var courseId = Guid.NewGuid();
        var orgId = 8;
        var userId = Guid.NewGuid().ToString();
        var course = new Course
        {
            CourseId = courseId,
            OrganizationId = orgId,
            StatusId = 1,
            Created = DateTime.UtcNow
        };

        var updateDto = new Dictionary<string, dynamic> { { "MinimumPercentageToPass", 80 } };

        _mockCurrentUserService
            .Setup(s => s.OrganizationId)
            .Returns(orgId.ToString());

        _mockCourseRepository
            .Setup(repo => repo.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken))
            .ReturnsAsync(course);

        _mockRepository
            .Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync((FinalExam?)null);

        _mockRepository
            .Setup(r => r.UpdateFinalExamAsync(courseId, userId, updateDto, _cancellationToken))
            .ThrowsAsync(new BadRequestException($"Final exam with course id {courseId} does not exist!"));

        _mockCurrentUserService.Setup(c => c.UserId).Returns(userId);

        var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.UpdateFinalExamAsync(courseId, orgId, updateDto, _cancellationToken));

        Assert.Equal($"Final exam with course id {courseId} does not exist!", ex.Message);
        _mockRepository.Verify(r => r.UpdateFinalExamAsync(courseId, userId, It.IsAny<Dictionary<string, dynamic>>(), _cancellationToken), Times.Once);
        _mockMapper.Verify(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()), Times.Never);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task UpdateFinalExamAsync_WhenCourseDoesNotExist_ThrowsBadRequest()
    {
        var courseId = Guid.NewGuid();
        var orgId = 8;
        var userId = Guid.NewGuid().ToString();
        var updateDto = new Dictionary<string, dynamic>
        {
            { "MinimumPercentageToPass", 75 },
            { "QuestionsDisplayedPerExam", 5 },
            { "Duration", new Duration { Hours = 0, Minutes = 30 } }
        };

        _mockCurrentUserService
            .Setup(s => s.OrganizationId)
            .Returns(orgId.ToString());

        _mockCourseRepository
            .Setup(repo => repo.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken))
            .ReturnsAsync((Course?)null);

        var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.UpdateFinalExamAsync(courseId, orgId, updateDto, _cancellationToken));
        Assert.Equal($"Unable to find course with course id {courseId}", ex.Message);

        _mockCourseRepository.Verify(r => r.GetCourseByCourseIdAsync(courseId, orgId, _cancellationToken), Times.Once);
        _mockRepository.Verify(r => r.GetFinalExamByCourseIdAsync(It.IsAny<Guid>(), _cancellationToken), Times.Never);
        _mockRepository.Verify(r => r.UpdateFinalExamAsync(It.IsAny<Guid>(), userId, It.IsAny<Dictionary<string, dynamic>>(), _cancellationToken), Times.Never);
        _mockMapper.Verify(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()), Times.Never);
        _mockMapper.Verify(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()), Times.Never);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task DeleteFinalExamAsync_ReturnsTrue_WhenFinalExamDeleted()
    {
        var finalExam = new FinalExam
        {
            CourseId = Guid.NewGuid(),
            MinimumPercentageToPass = 50,
            Created = DateTime.UtcNow
        };

        _mockRepository.Setup(repo => repo.GetFinalExamByCourseIdAsync(It.IsAny<Guid>(), _cancellationToken)).ReturnsAsync(finalExam);

        await _service.DeleteFinalExamAsync(finalExam.CourseId, _cancellationToken);

        _mockRepository.Verify(repo => repo.DeleteFinalExamAsync(finalExam, _cancellationToken), Times.Once);
        _mockMapper.Verify(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()), Times.Never);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(finalExam.CourseId, _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task DeleteFinalExamAsync_ThrowsNotFoundException_WhenFinalExamNotFound()
    {
        var courseId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetFinalExamByCourseIdAsync(It.IsAny<Guid>(), _cancellationToken)).ReturnsAsync((FinalExam?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteFinalExamAsync(courseId, _cancellationToken));

        Assert.Equal($"Final Exam not found.", ex.Message);

        _mockRepository.Verify(repo => repo.DeleteFinalExamAsync(It.IsAny<FinalExam>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Never);

    }
    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldAddQuestion_WhenValid()
    {
        var courseId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var questionId = Guid.NewGuid();


        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Add,
                QuestionId = questionId,
                QuestionType = QuestionType.SingleSelect,
                QuestionText = new LocalizedStringDto { En = "What is FinalExamStucture?" }
            }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockRepository.Setup(r => r.UpdateFinalExamQuestionAsync(It.IsAny<FinalExam>(), It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper.Setup(m => m.Map<LocalizedString>(It.IsAny<LocalizedStringDto>()))
            .Returns(new LocalizedString { En = updateDto.QuestionUpdate.QuestionText!.En });

        _mockMapper.Setup(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()))
            .Returns(new FinalExamDto());

        var result = await _service.UpdateFinalExamQuestionAsync(courseId, examId, questionId, updateDto, _cancellationToken);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Once);
        
        Assert.NotNull(result);
        Assert.NotNull(finalExam);
        Assert.NotNull(finalExam.FinalExamQuestions);
        Assert.Single(finalExam.FinalExamQuestions);

        var question = finalExam.FinalExamQuestions.FirstOrDefault();
        Assert.NotNull(question);
        Assert.NotNull(question.QuestionText);

        Assert.Equal("What is FinalExamStucture?", question.QuestionText.En);
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldReplaceQuestion_WhenValid()
    {
        var questionId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
        {
            new() { QuestionId = questionId, QuestionText = new LocalizedString { En = "Old Question" }, QuestionType = QuestionType.SingleSelect, QuestionOptions = [] }
        }
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Replace,
                QuestionId = questionId,
                QuestionText = new LocalizedStringDto { En = "Updated Question" },
                QuestionType = QuestionType.SingleSelect,

            }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockRepository.Setup(r => r.UpdateFinalExamQuestionAsync(It.IsAny<FinalExam>(), It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper.Setup(m => m.Map(It.IsAny<FinalExamUpdateQuestionDto>(), It.IsAny<FinalExamQuestion>()))
            .Callback<FinalExamUpdateQuestionDto, FinalExamQuestion>((src, dest) =>
          {
              dest.QuestionText = new LocalizedString
              {
                  En = src.QuestionText?.En ?? string.Empty
              };
          });
        _mockMapper.Setup(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()))
            .Returns(new FinalExamDto());

        var result = await _service.UpdateFinalExamQuestionAsync(courseId, examId, questionId, updateDto, _cancellationToken);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Once);

        Assert.NotNull(result);
        Assert.Equal("Updated Question", finalExam.FinalExamQuestions.First().QuestionText.En);
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldRemoveQuestion_WhenExists()
    {
        var questionId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
        {
            new() { QuestionId = questionId, QuestionText = new LocalizedString { En = "To be removed" } }
        }
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Remove,
                QuestionId = questionId
            }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockRepository.Setup(r => r.UpdateFinalExamQuestionAsync(It.IsAny<FinalExam>(), It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper.Setup(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()))
            .Returns(new FinalExamDto());

        var result = await _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), questionId, updateDto, _cancellationToken);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Once);
        Assert.NotNull(result);
        Assert.Empty(finalExam.FinalExamQuestions);
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldThrow_WhenRemovingOptionWithOnlyTwoOptions()
    {
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
        {
            new()
            {
                QuestionId = questionId,
                QuestionText = new LocalizedString { En = "Question" },
                QuestionType = QuestionType.SingleSelect,
                QuestionOptions = new List<FinalExamQuestionOptions>
                {
                    new() { OptionId = optionId, OptionText= new LocalizedString { En = "Option1" }},
                    new() { OptionId = Guid.NewGuid() , OptionText= new LocalizedString { En = "Option2" }}
                }
            }
        }
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new() { Action = PatchAction.Remove, OptionId = optionId }
        }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), questionId, updateDto, _cancellationToken));
        
        Assert.Equal("Cannot remove option. A question must have at least two options.", ex.Message);
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldAddOption_WhenValid()
    {
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
        {
            new()
            {
                QuestionId = questionId,
                QuestionText = new LocalizedString { En = "Question" },
                QuestionType = QuestionType.SingleSelect,
                QuestionOptions = new List<FinalExamQuestionOptions>()
            }
        }
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new()
            {
                Action = PatchAction.Add,
                OptionId = optionId,
                OptionText = new LocalizedStringDto { En = "Option Text" },
                ResponseFeedback =  new LocalizedStringDto { En = "Option Response Feedback" },
                IsCorrect = true
            }
        }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockRepository.Setup(r => r.UpdateFinalExamQuestionAsync(It.IsAny<FinalExam>(), It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper.Setup(m => m.Map<LocalizedString>(It.IsAny<LocalizedStringDto>()))
            .Returns<LocalizedStringDto>(dto => new LocalizedString { En = dto.En! });

        _mockMapper.Setup(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()))
            .Returns(new FinalExamDto());

        var result = await _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), questionId, updateDto, _cancellationToken);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Once);

        Assert.NotNull(result);
        Assert.NotNull(finalExam);
        Assert.NotNull(finalExam.FinalExamQuestions);
        Assert.Single(finalExam.FinalExamQuestions);

        var question = finalExam.FinalExamQuestions.Single();
        Assert.NotNull(question.QuestionOptions);
        Assert.Single(question.QuestionOptions);

        var option = question.QuestionOptions.Single();
        Assert.True(option.IsCorrect);
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldReplaceOption_WhenValid()
    {
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
        {
            new()
            {
                QuestionId = questionId,
                QuestionText = new LocalizedString { En = "Question" },
                QuestionType = QuestionType.SingleSelect,
                QuestionOptions = new List<FinalExamQuestionOptions>
                {
                    new() { OptionId = optionId, OptionText = new LocalizedString { En = "Old Text" }, IsCorrect = false }
                }
            }
        }
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new()
            {
                Action = PatchAction.Replace,
                OptionId = optionId,
                OptionText = new LocalizedStringDto { En = "Updated Text" },
                IsCorrect = true
            }
        }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockRepository.Setup(r => r.UpdateFinalExamQuestionAsync(It.IsAny<FinalExam>(), It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper.Setup(m => m.Map(It.IsAny<FinalExamQuestionUpdateOptionDto>(), It.IsAny<FinalExamQuestionOptions>()))
            .Callback<FinalExamQuestionUpdateOptionDto, FinalExamQuestionOptions>((src, dest) =>
            {
                dest.OptionText = new LocalizedString
                {
                    En = src.OptionText?.En ?? string.Empty
                };
                dest.IsCorrect = src.IsCorrect;
            });

        _mockMapper.Setup(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>()))
            .Returns(new FinalExamDto());

        var result = await _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), questionId, updateDto, _cancellationToken);
          _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Once);
        Assert.NotNull(result);
        Assert.NotNull(finalExam);
        Assert.NotNull(finalExam.FinalExamQuestions);
        Assert.Single(finalExam.FinalExamQuestions);

        var question = finalExam.FinalExamQuestions.First();
        Assert.NotNull(question.QuestionOptions);
        Assert.Single(question.QuestionOptions);

        var updatedOption = question.QuestionOptions.First();
        Assert.Equal("Updated Text", updatedOption?.OptionText?.En);
        Assert.True(updatedOption?.IsCorrect);
    }
    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldThrow_WhenReplacingNonExistentOption()
    {
        var questionId = Guid.NewGuid();
        var missingOptionId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
        {
            new()
            {
                QuestionId = questionId,
                QuestionText = new LocalizedString { En = "Question" },
                QuestionType = QuestionType.SingleSelect,
                QuestionOptions = new List<FinalExamQuestionOptions>() // Empty list
            }
        }
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new()
            {
                Action = PatchAction.Replace,
                OptionId = missingOptionId,
                OptionText = new LocalizedStringDto { En = "Updated Text" }
            }
        }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), questionId, updateDto, _cancellationToken));

        Assert.Equal($"Option not found in Question. OptionId: {missingOptionId}", ex.Message);
    }
    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldThrowBadRequestException_WhenFinalExamNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var patchDto = new FinalExamQuestionPatchDto();

        _mockRepository
            .Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(null as FinalExam);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateFinalExamQuestionAsync(courseId, examId, questionId, patchDto, _cancellationToken));
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldCallUpdateQuestion_WhenQuestionUpdateIsProvided()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
        };

        var patchDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Add,
                QuestionId = questionId,
                QuestionType = QuestionType.SingleSelect,
                QuestionText = new LocalizedStringDto { En = "What is FinalExamStucture?" }
            }
        };
        _mockRepository
            .Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockRepository
            .Setup(r => r.UpdateFinalExamQuestionAsync(finalExam, It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper
            .Setup(m => m.Map<FinalExamDto>(finalExam))
            .Returns(new FinalExamDto());

        // Act
        var result = await _service.UpdateFinalExamQuestionAsync(courseId, examId, questionId, patchDto, _cancellationToken);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Once);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.UpdateFinalExamQuestionAsync(finalExam, It.IsAny<string>(), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldUpdateOptions_WhenOptionsProvided()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
        };

        var patchDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Add,
                QuestionId = questionId,
                QuestionType = QuestionType.SingleSelect,
                QuestionText = new LocalizedStringDto { En = "What is FinalExamStucture?" }
            },

            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
            {
            new()
            {
                Action = PatchAction.Add,
                OptionId = Guid.NewGuid(),
                OptionText = new LocalizedStringDto { En = "Option Text" },
                ResponseFeedback =  new LocalizedStringDto { En = "Option Response Feedback" },
                IsCorrect = true
            },
            new()
            {
                Action = PatchAction.Add,
                OptionId = Guid.NewGuid(),
                OptionText = new LocalizedStringDto { En = "Option Text" },
                ResponseFeedback =  new LocalizedStringDto { En = "Option Response Feedback" },
                IsCorrect = false
            }
            }

        };
        _mockRepository
            .Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
                .ReturnsAsync(finalExam);

        _mockRepository
            .Setup(r => r.UpdateFinalExamQuestionAsync(finalExam, It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(finalExam);

        _mockMapper
            .Setup(m => m.Map<FinalExamDto>(finalExam))
                .Returns(new FinalExamDto());

        // Act
        var result = await _service.UpdateFinalExamQuestionAsync(courseId, examId, questionId, patchDto, _cancellationToken);
        
        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.UpdateFinalExamQuestionAsync(finalExam, It.IsAny<string>(), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldGenerateNewId_WhenQuestionIdIsEmpty()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var questionId = Guid.Empty; // should generate a new one

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
        };

        var patchDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Add,
                QuestionId = questionId,
                QuestionType = QuestionType.SingleSelect,
                QuestionText = new LocalizedStringDto { En = "What is FinalExamStucture?" }
            }
        };

        _mockRepository
            .Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockRepository
            .Setup(r => r.UpdateFinalExamQuestionAsync(finalExam, It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper
            .Setup(m => m.Map<FinalExamDto>(finalExam))
            .Returns(new FinalExamDto());

        // Act
        var result = await _service.UpdateFinalExamQuestionAsync(courseId, examId, questionId, patchDto, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.UpdateFinalExamQuestionAsync(finalExam, It.IsAny<string>(), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldThrowBadRequestException_WhenFinalExamIsNull()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var patchDto = new FinalExamQuestionPatchDto();

        _mockRepository
            .Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(null as FinalExam); // simulate not found

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateFinalExamQuestionAsync(courseId, examId, questionId, patchDto, _cancellationToken));

        Assert.Equal($"Unable to find final exam with course id  {courseId}", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateFinalExamQuestionAsync_ShouldThrow_WhenQuestionTextIsNullOrEmpty(string text)
    {
        var courseId = Guid.NewGuid();
        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Add,
                QuestionText = new LocalizedStringDto { En = text }
            }
        }; ;

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), Guid.NewGuid(), updateDto, _cancellationToken));

        Assert.Equal("Question Text is required.", ex.Message);
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldThrow_WhenQuestionLimitReached()
    {
        var courseId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = Enumerable.Range(0, 200).Select(i =>
                new FinalExamQuestion
                {
                    QuestionId = Guid.NewGuid(),
                    QuestionText = new LocalizedString { En = $"Q{i}" }
                }).ToList()
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionUpdate = new FinalExamUpdateQuestionDto
            {
                Action = PatchAction.Add,
                QuestionId = questionId,
                QuestionType = QuestionType.SingleSelect,
                QuestionText = new LocalizedStringDto { En = "What is finalexam structure?" }
            }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), Guid.NewGuid(), updateDto, _cancellationToken));

        Assert.Equal("Cannot add more than 200 questions.", ex.Message);
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldThrow_WhenSingleSelectAlreadyHasCorrectOption()
    {
        var questionId = Guid.NewGuid();
        var existingCorrectOptionId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
            {
            new()
            {
                QuestionId = questionId,
                QuestionText = new LocalizedString { En = "What is final exam structure?" },
                QuestionType = QuestionType.SingleSelect,
                QuestionOptions = new List<FinalExamQuestionOptions>
                {
                    new FinalExamQuestionOptions
                    {
                        OptionId = existingCorrectOptionId,
                        OptionText = new LocalizedString { En = "Existing correct" },
                        IsCorrect = true
                    }
                }
            }
        }
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new()
            {
                Action = PatchAction.Add,
                OptionId = Guid.NewGuid(),
                OptionText = new LocalizedStringDto { En = "New Option" },
                IsCorrect = true
            }
        }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), questionId, updateDto, _cancellationToken));
    }
    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldThrow_WhenQuestionNotFound()
    {
        var courseId = Guid.NewGuid();
        var questionId = Guid.NewGuid(); // Question ID that does NOT exist

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>() // empty
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new()
            {
                Action = PatchAction.Add,
                OptionId = Guid.NewGuid(),
                OptionText = new LocalizedStringDto { En = "New Option" },
                IsCorrect = false
            }
        }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), questionId, updateDto, _cancellationToken));
    }
    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldThrow_WhenOnlyTwoOptionsExist()
    {
        var courseId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var optionToRemove = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
        {
            new FinalExamQuestion
            {
                QuestionId = questionId,
                QuestionText = new LocalizedString { En = "What is final exam structure?" },
                QuestionType = QuestionType.SingleSelect,
                QuestionOptions = new List<FinalExamQuestionOptions>
                {
                    new() { OptionId = optionToRemove,OptionText = new LocalizedString { En = "New Option1" },
                IsCorrect = false },
                    new() { OptionId = Guid.NewGuid(),OptionText = new LocalizedString { En = "New Option2" },
                IsCorrect = false }
                }
            }
        }
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new() { Action = PatchAction.Remove, OptionId = optionToRemove }
        }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(finalExam);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), questionId, updateDto, _cancellationToken));
    }

    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldThrow_WhenRemovingOnlyCorrectOption()
    {
        var courseId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var correctOptionId = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
        {
            new FinalExamQuestion
            {
                QuestionId = questionId,
                QuestionText = new LocalizedString { En = "What is final exam structure?" },
                QuestionType = QuestionType.SingleSelect,
                QuestionOptions = new List<FinalExamQuestionOptions>
                {
                    new() { OptionId = Guid.NewGuid(), OptionText = new LocalizedString { En = "New Option1" },
                    IsCorrect = false },
                    new() { OptionId = Guid.NewGuid(),OptionText = new LocalizedString { En = "New Option2" }, IsCorrect = false },
                    new() { OptionId = correctOptionId, OptionText = new LocalizedString { En = "New Option3" },IsCorrect = true }
                }
            }
        }
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new() { Action = PatchAction.Remove, OptionId = correctOptionId }
        }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(finalExam);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), questionId, updateDto, _cancellationToken));
    }
    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldRemoveOption_WhenValid()
    {
        var courseId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var removableOptionId = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
        {
            new FinalExamQuestion
            {
                QuestionId = questionId,
                QuestionText = new LocalizedString { En = "What is final exam structure?" },
                QuestionType = QuestionType.MultiSelect,
                QuestionOptions = new List<FinalExamQuestionOptions>
                {
                    new() { OptionId = removableOptionId,OptionText = new LocalizedString { En = "New Option1" }, IsCorrect = false },
                    new() { OptionId = Guid.NewGuid(),OptionText = new LocalizedString { En = "New Option2" }, IsCorrect = true },
                    new() { OptionId = Guid.NewGuid(),OptionText = new LocalizedString { En = "New Option3" }, IsCorrect = true }
                }
            }
        }
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new() { Action = PatchAction.Remove, OptionId = removableOptionId }
        }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(finalExam);
        _mockRepository.Setup(r => r.UpdateFinalExamQuestionAsync(It.IsAny<FinalExam>(),It.IsAny<string>(), _cancellationToken)).ReturnsAsync(finalExam);
        _mockMapper.Setup(m => m.Map<FinalExamDto>(It.IsAny<FinalExam>())).Returns(new FinalExamDto());

        var result = await _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), questionId, updateDto, _cancellationToken);
        _mockCourseRepository.Verify(repo => repo.TriggerCourseContentUpdateAsync(courseId, _cancellationToken), Times.Once);
        var question = finalExam.FinalExamQuestions.FirstOrDefault();
        Assert.Equal(2, question?.QuestionOptions?.Count);
        Assert.DoesNotContain(question?.QuestionOptions!, o => o.OptionId == removableOptionId);
    }
    [Fact]
    public async Task UpdateFinalExamQuestionAsync_ShouldThrow_WhenOptionToRemoveNotFound()
    {
        var courseId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var nonExistentOptionId = Guid.NewGuid();

        var finalExam = new FinalExam
        {
            CourseId = courseId,
            QuestionsDisplayedPerExam = 40,
            Created = DateTime.UtcNow,
            FinalExamQuestions = new List<FinalExamQuestion>
        {
            new FinalExamQuestion
            {
                QuestionId = questionId,
                QuestionText = new LocalizedString { En = "What is final exam structure?" },
                QuestionType = QuestionType.SingleSelect,
                QuestionOptions = new List<FinalExamQuestionOptions>
                {
                    new() { OptionId = Guid.NewGuid(),OptionText = new LocalizedString { En = "New Option1" }, IsCorrect = true },
                    new() { OptionId = Guid.NewGuid(),OptionText = new LocalizedString { En = "New Option2" }, IsCorrect = false },
                    new() { OptionId = Guid.NewGuid(),OptionText = new LocalizedString { En = "New Option3" }, IsCorrect = false }
                }
            }
        }
        };

        var updateDto = new FinalExamQuestionPatchDto
        {
            QuestionOptionsUpdate = new List<FinalExamQuestionUpdateOptionDto>
        {
            new() { Action = PatchAction.Remove, OptionId = nonExistentOptionId }
        }
        };

        _mockRepository.Setup(r => r.GetFinalExamByCourseIdAsync(courseId, _cancellationToken)).ReturnsAsync(finalExam);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateFinalExamQuestionAsync(courseId, Guid.NewGuid(), questionId, updateDto, _cancellationToken));
    }

    [Fact]
    public async Task GetQuestionsByIdsAsync_ReturnsFinalExamDto_WhenIsCompletedIsTrue()
    {
        // Arrange  
        var lang = "en";
        Guid courseId = Guid.NewGuid();
        List<Guid> questionIds = new() { Guid.NewGuid() };
        bool isCompleted = true;
        CancellationToken cancellationToken = CancellationToken.None;
        var finalExam = new FinalExam
        {
            CourseId = courseId,
            FinalExamQuestions = new List<FinalExamQuestion>(),  
            Created = DateTime.UtcNow 
        };
        var finalExamDto = new FinalExamDto();

        _mockRepository
            .Setup(r => r.GetLearnerFinalExamByCourseIdAsync(courseId, questionIds, isCompleted, cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper
            .Setup(m => m.Map<FinalExamDto>(finalExam))
            .Returns(finalExamDto);

        // Act  
        var result = await _service.GetQuestionsByIdsAsync(courseId, questionIds, isCompleted, lang, cancellationToken);

        // Assert  
        Assert.IsType<FinalExamDto>(result);
        Assert.Equal(finalExamDto, result);
    }

    [Fact]
    public async Task GetQuestionsByIdsAsync_ReturnsFinalExamLearnerDto_WhenIsCompletedIsFalse()
    {
        // Arrange
        var lang = "en";
        Guid courseId = Guid.NewGuid();
        List<Guid> questionIds = new() { Guid.NewGuid() };
        bool isCompleted = false;
        CancellationToken cancellationToken = CancellationToken.None;
        var finalExam = new FinalExam { CourseId = courseId, FinalExamQuestions = new(), Created = DateTime.UtcNow };
        var finalExamLearnerDto = new FinalExamLearnerDto();

        _mockRepository
            .Setup(r => r.GetLearnerFinalExamByCourseIdAsync(courseId, questionIds, isCompleted, cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper
            .Setup(m => m.Map<FinalExamLearnerDto>(finalExam))
            .Returns(finalExamLearnerDto);

        // Act
        var result = await _service.GetQuestionsByIdsAsync(courseId, questionIds, isCompleted, lang, cancellationToken);

        // Assert
        Assert.IsType<FinalExamLearnerDto>(result);
        Assert.Equal(finalExamLearnerDto, result);
    }

    [Fact]
    public async Task GetFinalExamByCourseIdAsync_WhenFinalExamQuestionOptionsIdsExists_ReturnsMappedFinalExamSetupDto()
    {
        var duration = new Duration
        {
            Hours = 1,
            Minutes = 30
        };

        var questionLocalizedString = new LocalizedString
        {
            En = "What is the main purpose of supervised learning?"
        };

        var optionsLocalizedString = new LocalizedString
        {
            En = "To find hidden patterns in unlabeled data"
        };

        var questionOptions = new List<FinalExamQuestionOptions>
        {
            new FinalExamQuestionOptions
            {
                OptionId = Guid.NewGuid(),
                OptionText = optionsLocalizedString,
                IsCorrect = true
            },
            new FinalExamQuestionOptions
            {
                OptionId = Guid.NewGuid(),
                OptionText = optionsLocalizedString,
                IsCorrect = false
            }
        };

        var questions = new List<FinalExamQuestion>
        {
            new FinalExamQuestion
            {
                QuestionId = Guid.NewGuid(),
                QuestionType = QuestionType.SingleSelect,
                QuestionText = questionLocalizedString,
                QuestionOptions = questionOptions
            }
        };

        var finalExam = new FinalExam
        {
            Id = "49d7806e-4477-4ed9-ae88-34054259a471",
            CourseId = Guid.NewGuid(),
            MinimumPercentageToPass = 70,
            QuestionsDisplayedPerExam = 10,
            Duration = duration,
            FinalExamQuestions = questions,
            Created = DateTime.UtcNow,
            CreatedBy = "11111111-1111-1111-1111-111111111111"
        };


        var questionAnswerPairDto = new List<QuestionAnswerPairDto>
        {
            new QuestionAnswerPairDto
            {
                QuestionId = Guid.NewGuid(),
                CorrectOptionIds = new List<Guid>(){ Guid.NewGuid() }
            }
        };
        var expectedDto = new FinalExamSettingWithAnswerDto
        {
            MinimumPercentageToPass = (int)finalExam.MinimumPercentageToPass,
            QuestionsDisplayedPerExam = (int)finalExam.QuestionsDisplayedPerExam,
            Duration = duration,
            QuestionAnswerPairs = questionAnswerPairDto
        };


        _mockRepository
            .Setup(repo => repo.GetLearnerFinalExamQuestionAnswerIdsAsync(finalExam.CourseId, _cancellationToken))
            .ReturnsAsync(finalExam);

        _mockMapper
        .Setup(mapper => mapper.Map<FinalExamSettingWithAnswerDto>(finalExam))
            .Returns(expectedDto);

        var result = await _service.GetLearnerFinalExamQuestionAnswerIdsAsync(finalExam.CourseId, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedDto.MinimumPercentageToPass, result.MinimumPercentageToPass);
        Assert.Equal(expectedDto.QuestionsDisplayedPerExam, result.QuestionsDisplayedPerExam);
        Assert.Equal(expectedDto.Duration.Hours, result.Duration!.Hours);
        Assert.Equal(expectedDto.Duration.Minutes, result.Duration!.Minutes);
        Assert.NotNull(result.QuestionAnswerPairs);
        Assert.NotEmpty(result.QuestionAnswerPairs);

        var question = result.QuestionAnswerPairs[0];
        Assert.NotEmpty(question.CorrectOptionIds);
        Assert.NotNull(question.CorrectOptionIds);

        _mockRepository.Verify(repo => repo.GetLearnerFinalExamQuestionAnswerIdsAsync(finalExam.CourseId, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetLearnerFinalExamQuestionAnswerIdsAsync_ReturnsNull_WhenFinalExamQuestionsNull()
    {
        var courseId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetLearnerFinalExamQuestionAnswerIdsAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FinalExam {
                CourseId = courseId,
                Duration = new Duration { Hours = 1, Minutes = 30 },
                QuestionsDisplayedPerExam = 10,
                MinimumPercentageToPass = 70,
                Created = DateTime.UtcNow,
                FinalExamQuestions = null 
            });

        var result = await _service.GetLearnerFinalExamQuestionAnswerIdsAsync(courseId, CancellationToken.None);

        Assert.Null(result);

    }

    [Fact]
    public async Task GetLearnerFinalExamQuestionAnswerIdsAsync_ReturnsNull_WhenFinalExamQuestionsEmpty()
    {
        var courseId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetLearnerFinalExamQuestionAnswerIdsAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FinalExam {
                CourseId = courseId,
                Duration = new Duration { Hours = 1, Minutes = 30 },
                QuestionsDisplayedPerExam = 10,
                MinimumPercentageToPass = 70,
                Created = DateTime.UtcNow,
                FinalExamQuestions = new List<FinalExamQuestion>() 
            });

        var result = await _service.GetLearnerFinalExamQuestionAnswerIdsAsync(courseId, CancellationToken.None);

        Assert.Null(result);
    }
}