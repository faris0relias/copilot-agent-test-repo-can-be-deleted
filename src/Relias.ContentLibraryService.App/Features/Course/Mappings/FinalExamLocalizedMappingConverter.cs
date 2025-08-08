using AutoMapper;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.App.Services;
using Relias.ContentLibraryService.Common.Utilities;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Features.Course.Mappings;

public class FinalExamLocalizedMappingConverter : ITypeConverter<FinalExam, FinalExamDto>
{
    private readonly ILanguageContextService _languageContextService;

    public FinalExamLocalizedMappingConverter(ILanguageContextService languageContextService)
    {
        _languageContextService = languageContextService;
    }
    public FinalExamDto Convert(FinalExam finalExam, FinalExamDto destination, ResolutionContext context)
    {
       string lang = _languageContextService.GetCurrentLanguage();
        var finalExamDto = new FinalExamDto
        {
            Id = Guid.Parse(finalExam.Id),
            Duration = finalExam.Duration,
            MinimumPercentageToPass = finalExam.MinimumPercentageToPass,
            QuestionsDisplayedPerExam = finalExam.QuestionsDisplayedPerExam,
            FinalExamQuestions = finalExam.FinalExamQuestions?.Select(q => new FinalExamQuestionDto
            {
                QuestionId = q.QuestionId,
                QuestionText = Localization.GetLocalizedText<LocalizedStringDto, LocalizedString>(q.QuestionText, lang),
                QuestionType = q.QuestionType,
                QuestionOptions = q.QuestionOptions?.Select(opt => new FinalExamQuestionOptionsDto
                {
                    OptionId = opt.OptionId,
                    IsCorrect = opt.IsCorrect,
                    ResponseFeedback = Localization.GetLocalizedText<LocalizedStringDto, LocalizedString>(opt.ResponseFeedback, lang),
                    OptionText = Localization.GetLocalizedText<LocalizedStringDto, LocalizedString>(opt.OptionText, lang)
                }).ToList()
            }).ToList()
        };

        return finalExamDto;
    }

}
