using AutoMapper;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Common.Utilities;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Features.Course.Mappings;

public class FinalExamLearnerLocalizedMappingConverter : ITypeConverter<FinalExam, FinalExamLearnerDto>
{
    private readonly ILanguageContextService _languageContextService;
    public FinalExamLearnerLocalizedMappingConverter(ILanguageContextService languageContextService)
    {
        _languageContextService = languageContextService;
    }
    public FinalExamLearnerDto Convert(FinalExam finalExam, FinalExamLearnerDto destination, ResolutionContext context)
    {
        string lang = _languageContextService.GetCurrentLanguage();
        var finalExamDto = new FinalExamLearnerDto
        {
            Id = Guid.Parse(finalExam.Id),
            Duration = finalExam.Duration,
            MinimumPercentageToPass = finalExam.MinimumPercentageToPass,
            QuestionsDisplayedPerExam = finalExam.QuestionsDisplayedPerExam,
            FinalExamQuestions = finalExam.FinalExamQuestions?.Select(q => new FinalExamQuestionLearnerDto
            {
                QuestionId = q.QuestionId,
                QuestionText = Localization.GetLocalizedText<LocalizedStringDto, LocalizedString>(q.QuestionText, lang),
                QuestionType = q.QuestionType,
                QuestionOptions = q.QuestionOptions?.Select(opt => new FinalExamQuestionOptionsLearnerDto
                {
                    OptionId = opt.OptionId,
                    OptionText = Localization.GetLocalizedText<LocalizedStringDto, LocalizedString>(opt.OptionText, lang)
                }).ToList()
            }).ToList()
        };

        return finalExamDto;
    }

}
