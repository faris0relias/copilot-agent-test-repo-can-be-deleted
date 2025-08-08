using AutoMapper;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.App.Features.Course.Mappings;

public class LearningContentMappingProfile : Profile
{
    public LearningContentMappingProfile()
    {
        CreateMap<LearningContent, LearningContentDto>().ReverseMap();
        
        CreateMap<LearningContentSection, LearningContentSectionDto>().ReverseMap();
        
        CreateMap<LearningObject, LearningObjectDto>()
            .Include<Lesson, LessonDto>();

        CreateMap<Lesson, LessonDto>().ReverseMap();

        CreateMap<LocalizedString, LocalizedStringDto>().ReverseMap();

        CreateMap<LearningObjectDto, LearningObject>()
            .Include<LessonDto, Lesson>();

        CreateMap<SectionUpdateDto, LearningContentSection>()
            .ForMember(dest => dest.LearningObjects, opt => opt.Ignore());

        CreateMap<LessonUpdateDto, Lesson>()
            .ForMember(dest => dest.LearningObjectType, opt => opt.Ignore());
    }
}
