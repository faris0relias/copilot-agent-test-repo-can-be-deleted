using AutoMapper;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Features.Content.Mappings;

namespace Relias.ContentLibraryService.App.Tests.Features.Content.Mappings;

public class ContentInfoMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _mapperConfig;

    public ContentInfoMappingProfileTests()
    {
        _mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<ContentInfoMappingProfile>());
        _mapper = _mapperConfig.CreateMapper();
    }

    [Fact]
    public void ContentInfoMappingProfile_ShouldBeValid()
    {
        _mapperConfig.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_CourseToContentInfoDto_ShouldMapCorrectly()
    {
        var course = new Domain.Course.Course
        {
            ContentId = Guid.NewGuid(),
            Title = "Test Course",
            TimeToCompleteInMinutes = 60,
            Created = DateTime.UtcNow
        };

        var result = _mapper.Map<ContentInfoDto>(course);

        Assert.NotNull(result);
        Assert.Equal(course.ContentId, result.ContentId);
        Assert.Equal(2, result.ContentTypeId);
        Assert.Equal(course.Title, result.Title);
        Assert.Equal(course.TimeToCompleteInMinutes, result.TimeToCompleteInMinutes);
    }
}
