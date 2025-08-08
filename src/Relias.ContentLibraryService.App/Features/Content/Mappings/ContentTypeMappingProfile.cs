using AutoMapper;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.Domain.ContentType;

namespace Relias.ContentLibraryService.App.Features.Content.Mappings;

public class ContentTypeMappingProfile : Profile
{
    public ContentTypeMappingProfile()
    {
        CreateMap<ContentType, ContentTypeDto>().ReverseMap();
    }
}
