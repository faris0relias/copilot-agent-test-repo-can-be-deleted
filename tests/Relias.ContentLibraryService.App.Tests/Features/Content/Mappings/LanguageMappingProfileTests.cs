using AutoMapper;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Features.Content.Mappings;
using Relias.ContentLibraryService.Domain.Language;

namespace Relias.ContentLibraryService.App.Tests.Features.Content.Mappings;

public class LanguageMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _mapperConfig;

    public LanguageMappingProfileTests()
    {
        _mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<LanguageMappingProfile>());
        _mapper = _mapperConfig.CreateMapper();
    }

    [Fact]
    public void LanguageMappingProfile_ShouldBeValid()
    {
        _mapperConfig.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_LanguageToLanguageDto_ShouldMapCorrectly()
    {
        var language = new Language
        {
            LanguageId = Guid.NewGuid(),
            Code = "en",
            Name = "English"
        };

        var result = _mapper.Map<LanguageDto>(language);

        Assert.NotNull(result);
        Assert.Equal(language.LanguageId, result.LanguageId);
        Assert.Equal(language.Code, result.Code);
        Assert.Equal(language.Name, result.Name);
    }

    [Fact]
    public void Map_LanguageDtoToLanguage_ShouldMapCorrectly()
    {
        var dto = new LanguageDto
        {
            LanguageId = Guid.NewGuid(),
            Code = "sp",
            Name = "Spanish"
        };

        var result = _mapper.Map<Language>(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.LanguageId, result.LanguageId);
        Assert.Equal(dto.Code, result.Code);
        Assert.Equal(dto.Name, result.Name);
    }
}

