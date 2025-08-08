using AutoMapper;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.ReliasCourse;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.App.Features.Course.Mappings;

public class CourseMappingProfile : Profile
{
    public CourseMappingProfile()
    {
        CreateMap<Domain.Course.Course, CourseDto>()
            .ForMember(dest => dest.ReliasCourseProperties, opt => opt.MapFrom(src => src.ReliasCourseProperties));

        CreateMap<CourseDto, Domain.Course.Course>()
            .ForMember(dest => dest.ReliasCourseProperties, opt => opt.Ignore());

        CreateMap<ReliasCourseProperties, ReliasCoursePropertiesDto>()
            .ForMember(dest => dest.CommercialProductDisclaimer, opt => opt.MapFrom(src => src.CommercialProductDisclaimer != null ? src.CommercialProductDisclaimer.Disclaimer : null))
            .ForMember(dest => dest.CompletionRequirement, opt => opt.MapFrom(src => src.CompletionRequirement != null ? src.CompletionRequirement.Requirement : null))
            .ForMember(dest => dest.ContentDisclaimer, opt => opt.MapFrom(src => src.ContentDisclaimer != null ? src.ContentDisclaimer.Disclaimer : null))
            .ForMember(dest => dest.CulturalAwarenessStatement, opt => opt.MapFrom(src => src.CulturalAwarenessStatement != null ? src.CulturalAwarenessStatement.Statement : null))
            .ForMember(dest => dest.RequestForAccommodations, opt => opt.MapFrom(src => src.RequestForAccommodations != null ? src.RequestForAccommodations.Request : null))
            .ForMember(dest => dest.LearningObjectives, opt => opt.MapFrom(src => src.LearningObjectives.Select(lo => lo.Objective)))
            .ForMember(dest => dest.CareSettings, opt => opt.MapFrom(src => src.CareSettings.Select(cs => cs.CareSetting.Setting)))
            .ForMember(dest => dest.TargetAudiences, opt => opt.MapFrom(src => src.TargetAudiences.Select(ta => ta.TargetAudience.Audience)))
            .ForMember(dest => dest.TrainingTopics, opt => opt.MapFrom(src => src.TrainingTopics.Select(tt => tt.TrainingTopic.Topic)))
            .ForMember(dest => dest.Disclosures, opt => opt.MapFrom(src => src.Disclosures.Select(d => d.Disclosure.Statement)));

        CreateMap<CourseContributor, ContributorDto>()
            .ForMember(dest => dest.ContributorIdNum, opt => opt.MapFrom(src => src.Contributor.ContributorId))
            .ForMember(dest => dest.NameAndCredentials, opt => opt.MapFrom(src => src.Contributor.NameAndCredentials))
            .ForMember(dest => dest.ShortBio, opt => opt.MapFrom(src => src.Contributor.ShortBio))
            .ForMember(dest => dest.DisclosureStatement, opt => opt.MapFrom(src => src.Contributor.DisclosureStatement));
    }
}
