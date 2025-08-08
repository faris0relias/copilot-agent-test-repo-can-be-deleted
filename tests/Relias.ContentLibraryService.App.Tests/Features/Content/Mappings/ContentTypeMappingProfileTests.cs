using AutoMapper;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Features.Content.Mappings;
using Relias.ContentLibraryService.Domain.ContentType;

namespace Relias.ContentLibraryService.App.Tests.Features.Content.Mappings;

public class ContentTypeMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _mapperConfig;

    public ContentTypeMappingProfileTests()
    {
        _mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<ContentTypeMappingProfile>());
        _mapper = _mapperConfig.CreateMapper();
    }

    [Fact]
    public void ContentTypeMappingProfile_ShouldBeValid()
    {
        _mapperConfig.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_ContentTypeToContentTypeDto_ShouldMapCorrectly()
    {
        var contentType = new ContentType
        {
            ContentTypeId = 1,
            ContentTypeDescription = "Content Type 1"
        };

        var result = _mapper.Map<ContentTypeDto>(contentType);

        Assert.NotNull(result);
        Assert.Equal(contentType.ContentTypeId, result.ContentTypeId);
        Assert.Equal(contentType.ContentTypeDescription, result.ContentTypeDescription);
    }

    [Fact]
    public void Map_ContentTypeDtoToContentType_ShouldMapCorrectly()
    {
        var dto = new ContentTypeDto
        {
            ContentTypeId = 1,
            ContentTypeDescription = "Content Type 1"
        };

        var result = _mapper.Map<ContentType>(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.ContentTypeId, result.ContentTypeId);
        Assert.Equal(dto.ContentTypeDescription, result.ContentTypeDescription);
    }
}
