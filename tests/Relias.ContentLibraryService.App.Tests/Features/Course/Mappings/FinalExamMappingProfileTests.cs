using AutoMapper;
using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.App.Features.Course.Enums;
using Relias.ContentLibraryService.App.Features.Course.Mappings;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Mappings;
public class FinalExamMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _mapperConfig;
    

    public FinalExamMappingProfileTests()
    {
        var languageContextMock = new Mock<ILanguageContextService>();
        languageContextMock.Setup(x => x.GetCurrentLanguage()).Returns("en");

        _mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new FinalExamMappingProfile());
            cfg.ConstructServicesUsing(type =>
            {
                if (type == typeof(FinalExamLocalizedMappingConverter))
                    return new FinalExamLocalizedMappingConverter(languageContextMock.Object);

                if (type == typeof(FinalExamLearnerLocalizedMappingConverter))
                    return new FinalExamLearnerLocalizedMappingConverter(languageContextMock.Object);

                return Activator.CreateInstance(type);
            });
        });
        _mapper = _mapperConfig.CreateMapper();
    }

    [Fact]
    public void Convert_ShouldMapDictionaryToFinalExamDto()
    {
        var dataValues = new Dictionary<string, dynamic>
        {
            { "MinimumPercentageToPass", "85"},
            { "QuestionsDisplayedPerExam", "10"},
        };
        var result = _mapper.Map<FinalExamDto>(dataValues);
        Assert.NotNull(result);
        Assert.Equal(85, result.MinimumPercentageToPass);
        Assert.Equal(10, result.QuestionsDisplayedPerExam);
    }

    [Fact]
    public void Convert_WrongPropertyEntered_throwsError_()
    {
        var dataValues = new Dictionary<string, dynamic>
        {
            { "RandomValue", "85" },
            { "QuestionsDisplayedPerExam", "10" },
        };
        Assert.Throws<InvalidOperationException>(() => _mapper.Map<FinalExamDto>(dataValues));
    }
    [Fact]
    public void FinalExamQuestionUpdateOptionDto_To_FinalExamQuestionOptions_Should_Map_Correctly()
    {
        var dto = new FinalExamQuestionUpdateOptionDto
        {
            Action = PatchAction.Replace,
            OptionId = Guid.NewGuid(),
            OptionText = new LocalizedStringDto { En = "Option A" },
            ResponseFeedback = new LocalizedStringDto { En = "Correct answer!" },
            IsCorrect = true
        };

        var result = _mapper.Map<FinalExamQuestionOptions>(dto);

        Assert.Equal(dto.OptionText.En, result.OptionText?.En);
        Assert.Equal(dto.ResponseFeedback.En, result.ResponseFeedback?.En);
        Assert.Equal(dto.IsCorrect, result.IsCorrect);
    }

    [Fact]
    public void FinalExamUpdateQuestionDto_To_FinalExamQuestion_Should_Map_Correctly()
    {
        var dto = new FinalExamUpdateQuestionDto
        {
            Action = PatchAction.Replace,
            QuestionId = Guid.NewGuid(),
            QuestionText = new LocalizedStringDto { En = "What is Capital of USA?" },
            QuestionType = QuestionType.MultiSelect
        };

        var result = _mapper.Map<FinalExamQuestion>(dto);

        Assert.Equal(dto.QuestionText.En, result.QuestionText.En);
        Assert.Equal(dto.QuestionType, result.QuestionType);
    }

    [Fact]
    public void LocalizedStringDto_To_LocalizedString_Should_Map_Correctly()
    {
        var dto = new LocalizedStringDto
        {
            En = "Hello"
        };

        var result = _mapper.Map<LocalizedString>(dto);

        Assert.Equal(dto.En, result.En);
    }

    [Fact]
    public void FinalExamQuestionOptions_To_FinalExamQuestionUpdateOptionDto_Should_Reverse_Map_Correctly()
    {
        var entity = new FinalExamQuestionOptions
        {
            OptionId = Guid.NewGuid(),
            OptionText = new LocalizedString { En = "Option B" },
            ResponseFeedback = new LocalizedString { En = "Try again." },
            IsCorrect = false
        };

        var dto = _mapper.Map<FinalExamQuestionUpdateOptionDto>(entity);

        Assert.Equal(entity.OptionText.En, dto.OptionText?.En);
        Assert.Equal(entity.ResponseFeedback.En, dto.ResponseFeedback?.En);
        Assert.Equal(entity.IsCorrect, dto.IsCorrect);
    }

    [Fact]
    public void GetFinalExamByCourseIdAsync_WhenFinalExamWithQuestionsExists_ReturnsMappedFinalExamDto()
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
                ResponseFeedback = responseFeedbacksLocalizedString
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
                ResponseFeedback = responseFeedbackDto
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

        var result = _mapper.Map<FinalExamDto>(finalExam);
        
        Assert.NotNull(result);
        Assert.NotNull(result.FinalExamQuestions);
        Assert.NotEmpty(result.FinalExamQuestions);

        var question = result.FinalExamQuestions[0];
        Assert.NotNull(question.QuestionOptions);
        Assert.NotEmpty(question.QuestionOptions);

        Assert.Equal(expectedDto.FinalExamQuestions[0].QuestionText.En, question.QuestionText.En);
        Assert.Equal(expectedDto?.FinalExamQuestions?[0].QuestionOptions?[0].OptionText?.En, question.QuestionOptions[0].OptionText?.En);

    }

    [Fact]
    public void GetLearnerFinalExamByCourseIdAsync_WhenFinalExamWithQuestionsExists_ReturnsMappedFinalExamLearnerDto()
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
                OptionText = optionsLocalizedString

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

        
        var questionsOptionsDto = new List<FinalExamQuestionOptionsLearnerDto>
        {
            new FinalExamQuestionOptionsLearnerDto
            {
                OptionId = Guid.NewGuid(),
                OptionText = optionTextDto
            }
        };

        var questionTextDto = new LocalizedStringDto
        {
            En = "What is the main purpose of supervised learning?"
        };

        var questionsDto = new List<FinalExamQuestionLearnerDto>
        {
            new FinalExamQuestionLearnerDto
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

        var expectedDto = new FinalExamLearnerDto
        {
            MinimumPercentageToPass = finalExam.MinimumPercentageToPass,
            QuestionsDisplayedPerExam = finalExam.QuestionsDisplayedPerExam,
            Duration = duration,
            FinalExamQuestions = questionsDto
        };
        var result = _mapper.Map<FinalExamLearnerDto>(finalExam);

        Assert.NotNull(result);
        Assert.NotNull(result.FinalExamQuestions);
        Assert.NotEmpty(result.FinalExamQuestions);

        var question = result.FinalExamQuestions[0];
        Assert.NotNull(question.QuestionOptions);
        Assert.NotEmpty(question.QuestionOptions);

        Assert.Equal(expectedDto.FinalExamQuestions[0].QuestionText.En, question.QuestionText.En);
        Assert.Equal(expectedDto?.FinalExamQuestions?[0].QuestionOptions?[0].OptionText?.En, question.QuestionOptions[0].OptionText?.En);

    }

    [Fact]
    public void GetLearnerFinalExamByCourseIdAsync_WhenFinalExamWithQuestionsOptionsIdsExists_ReturnsMappedFinalExamSetupDto()
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

        
        var questionAnswerPairDto = new List<QuestionAnswerPairDto>
        {
            new QuestionAnswerPairDto
            {
                QuestionId = Guid.NewGuid(),
                CorrectOptionIds = new List<Guid>(){ Guid.NewGuid() }
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

        var expectedDto = new FinalExamSettingWithAnswerDto
        {
            MinimumPercentageToPass = (int)finalExam.MinimumPercentageToPass,
            QuestionsDisplayedPerExam = (int)finalExam.QuestionsDisplayedPerExam,
            Duration = duration,
            QuestionAnswerPairs = questionAnswerPairDto
        };
        var result = _mapper.Map<FinalExamSettingWithAnswerDto>(finalExam);

        Assert.NotNull(result);
        Assert.NotNull(result.QuestionAnswerPairs);
        Assert.NotEmpty(result.QuestionAnswerPairs);

        var question = result.QuestionAnswerPairs[0];
        Assert.NotEmpty(question.CorrectOptionIds);
        Assert.NotNull(question.CorrectOptionIds);

    }

    [Fact]
    public void GetLearnerFinalExamSettingByCourseIdAsync_WhenFinalExamExists_ReturnsMappedFinalExamSettingsDto()
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

        
        var result = _mapper.Map<FinalExamSettingDto>(finalExam);

        Assert.NotNull(result);

    }


    [Fact]
    public void GetCourseByIdAsync_WhenFinalExamWithQuestionsIsNull()
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
            CreatedBy = "11111111-1111-1111-1111-111111111111",
            FinalExamQuestions = null
        };

        var expectedDto = new FinalExamDto
        {
            MinimumPercentageToPass = finalExam.MinimumPercentageToPass,
            QuestionsDisplayedPerExam = finalExam.QuestionsDisplayedPerExam,
            Duration = duration,
            FinalExamQuestions = null
        };
        var result = _mapper.Map<FinalExamDto>(finalExam);

        Assert.NotNull(result);
        Assert.Null(result.FinalExamQuestions);

    }  
}