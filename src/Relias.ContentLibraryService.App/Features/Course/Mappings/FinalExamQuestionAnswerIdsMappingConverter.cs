using AutoMapper;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Features.Course.Mappings;

public class FinalExamQuestionAnswerIdsMappingConverter : ITypeConverter<FinalExam, FinalExamSettingWithAnswerDto>
{
    public FinalExamSettingWithAnswerDto Convert(FinalExam finalExam, FinalExamSettingWithAnswerDto destination, ResolutionContext context)
    {
        var finalExamDto = new FinalExamSettingWithAnswerDto
        {
            Id = Guid.Parse(finalExam.Id),
            Duration = finalExam.Duration ?? new Duration(), 
            MinimumPercentageToPass = finalExam.MinimumPercentageToPass ?? 0, 
            QuestionsDisplayedPerExam = finalExam.QuestionsDisplayedPerExam ?? 0,
            QuestionAnswerPairs = finalExam.FinalExamQuestions!.Select(q => new QuestionAnswerPairDto
            {
                QuestionId = q.QuestionId,
                CorrectOptionIds = q.QuestionOptions?
                .Where(opt => opt.IsCorrect == true && opt.OptionId != null)
                .Select(opt => opt.OptionId!.Value)
                .ToList() ?? new List<Guid>()
            }).ToList()
        };

        return finalExamDto;
    }
}
