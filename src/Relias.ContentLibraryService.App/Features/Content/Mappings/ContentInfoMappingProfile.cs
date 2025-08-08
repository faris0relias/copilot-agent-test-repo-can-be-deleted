using AutoMapper;
using Relias.ContentLibraryService.App.Features.Content.Dtos;

namespace Relias.ContentLibraryService.App.Features.Content.Mappings;

public class ContentInfoMappingProfile : Profile
{
    public ContentInfoMappingProfile()
    {
        CreateMap<Domain.Course.Course, ContentInfoDto>()
            .ForMember(m => m.ContentId, dest => dest.MapFrom(e => e.ContentId))
            .ForMember(m => m.ContentTypeId, dest => dest.MapFrom(e => 2)) // 2 == Course
            .ForMember(m => m.Title, dest => dest.MapFrom(e => e.Title))
            .ForMember(m => m.TimeToCompleteInMinutes, dest => dest.MapFrom(e => e.TimeToCompleteInMinutes));
    }
}
