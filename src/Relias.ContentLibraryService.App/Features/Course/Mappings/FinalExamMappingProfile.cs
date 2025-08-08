using AutoMapper;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Features.Course.Mappings;

public class FinalExamMappingProfile : Profile
{
    public FinalExamMappingProfile()
    {
        CreateMap<FinalExam, FinalExamDto>().ConvertUsing<FinalExamLocalizedMappingConverter>();

        CreateMap<FinalExam, FinalExamLearnerDto>().ConvertUsing<FinalExamLearnerLocalizedMappingConverter>();
        CreateMap<FinalExam, FinalExamSettingWithAnswerDto>().ConvertUsing<FinalExamQuestionAnswerIdsMappingConverter>();

        CreateMap<FinalExam, FinalExamSettingDto>().ReverseMap();

        CreateMap<Dictionary<string, dynamic>, FinalExamDto>().ConvertUsing<FinalExamMappingConverter>();

        CreateMap<FinalExamQuestion, FinalExamQuestionDto>().ReverseMap();
        CreateMap<FinalExamQuestion, FinalExamQuestionLearnerDto>().ReverseMap();

        CreateMap<FinalExamQuestionOptions, FinalExamQuestionOptionsDto>().ReverseMap();
        CreateMap<FinalExamQuestionOptions, FinalExamQuestionOptionsLearnerDto>().ReverseMap();

        CreateMap<FinalExamQuestionUpdateOptionDto, FinalExamQuestionOptions>()
          .ForMember(dest => dest.OptionText, opt => opt.MapFrom(src => src.OptionText))
          .ForMember(dest => dest.ResponseFeedback, opt => opt.MapFrom(src => src.ResponseFeedback))
          .ForMember(dest => dest.IsCorrect, opt => opt.MapFrom(src => src.IsCorrect))
          .ReverseMap();

        CreateMap<FinalExamUpdateQuestionDto, FinalExamQuestion>()
           .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.QuestionText))
           .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.QuestionType))
           .ReverseMap();

        CreateMap<LocalizedStringDto, LocalizedString>().ReverseMap();

    }

}

