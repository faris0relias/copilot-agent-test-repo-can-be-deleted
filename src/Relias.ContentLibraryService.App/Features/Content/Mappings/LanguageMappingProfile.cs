using AutoMapper;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.Domain.Language;

namespace Relias.ContentLibraryService.App.Features.Content.Mappings;

public class LanguageMappingProfile : Profile
{
    public LanguageMappingProfile() 
    {
        CreateMap<Language, LanguageDto>().ReverseMap();
    }
}
