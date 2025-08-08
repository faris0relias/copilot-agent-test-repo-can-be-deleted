using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Relias.ContentLibraryService.App.Features.Content.Mappings;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Mappings;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Mappings;

public class LearningContentMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _mapperConfig;

    public LearningContentMappingProfileTests()
    {
        _mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<LanguageMappingProfile>();
            cfg.AddProfile<LearningContentMappingProfile>();
      

        });
        _mapper = _mapperConfig.CreateMapper();
    }

    [Fact]
    public void LearningContentMappingProfile_ShouldBeValid()
    {
        _mapperConfig.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_LearningContentToLearningContentDto_ShouldMapCorrectly()
    {
        var lesson = new Lesson
        {
            LearningObjectId = Guid.NewGuid(),
            LearningObjectType = LearningObjectType.Lesson,
            LessonType = "file",
            Name = new LocalizedString { En = "Lesson Title" },
            DurationMinutes = 5,
            RequiredForCompletion = false,
            RequiresAudio = false,
            RequiresVideo = true,
            ContentPath = "https://blob.storage/file"
        };

        var section = new LearningContentSection
        {
            SectionId = Guid.NewGuid(),
            Name = new LocalizedString { En = "Section 1" },
            LearningObjects = [lesson]
        };

        var content = new LearningContent
        {
            Id = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            Sections = [section]
        };

        var result = _mapper.Map<LearningContentDto>(content);

        Assert.NotNull(result);
        Assert.Equal(content.Id, result.Id);
        Assert.Equal(content.CourseId, result.CourseId);
        Assert.Single(result.Sections);
        Assert.Equal("Section 1", result.Sections[0].Name.En);
        Assert.Single(result.Sections[0].LearningObjects);

        var lessonDto = Assert.IsType<LessonDto>(result.Sections[0].LearningObjects[0]);
        Assert.Equal(lesson.LearningObjectId, lessonDto.LearningObjectId);
    }

    [Fact]
    public void Map_LearningContentDtoToLearningContent_ShouldMapCorrectly()
    {
        var dto = new LearningContentDto
        {
            Id = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            Sections =
            [
                new LearningContentSectionDto
                {
                    SectionId = Guid.NewGuid(),
                    Name = new LocalizedStringDto { En = "Section 1" },
                    LearningObjects =
                    [
                        new LessonDto
                        {
                            LearningObjectId = Guid.NewGuid(),
                            LearningObjectType = (int)LearningObjectType.Lesson,
                            LessonType = "url",
                            Name = new LocalizedStringDto { En = "Lesson Title" },
                            DurationMinutes = 10,
                            RequiredForCompletion = true,
                            RequiresAudio = false,
                            RequiresVideo = true,
                            OpensInNewTab = true,
                            ContentPath = "https://example.com/lesson"
                        }
                    ]
                }
            ]
        };

        var result = _mapper.Map<LearningContent>(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.CourseId, result.CourseId);
        Assert.Single(result.Sections);

        var section = result.Sections[0];
        Assert.Equal(dto.Sections[0].SectionId, section.SectionId);
        Assert.Equal("Section 1", section.Name.En);
        Assert.Single(section.LearningObjects);

        var lesson = Assert.IsType<Lesson>(section.LearningObjects[0]);
        var lessonDto = (LessonDto)dto.Sections[0].LearningObjects[0];

        Assert.Equal(lessonDto.LearningObjectId, lesson.LearningObjectId);
        Assert.Equal(lessonDto.LessonType, lesson.LessonType);
        Assert.Equal("Lesson Title", lesson.Name.En);
        Assert.Equal(lessonDto.DurationMinutes, lesson.DurationMinutes);
        Assert.Equal(lessonDto.RequiredForCompletion, lesson.RequiredForCompletion);
        Assert.Equal(lessonDto.RequiresAudio, lesson.RequiresAudio);
        Assert.Equal(lessonDto.RequiresVideo, lesson.RequiresVideo);
        Assert.Equal(lessonDto.OpensInNewTab, lesson.OpensInNewTab);
        Assert.Equal(lessonDto.ContentPath, lesson.ContentPath);
    }

    [Fact]
    public void Map_LessonToLessonDto_ShouldMapCorrectly()
    {
        var lesson = new Lesson
        {
            LearningObjectId = Guid.NewGuid(),
            LearningObjectType = LearningObjectType.Lesson,
            LessonType = "url",
            Name = new LocalizedString { En = "Lesson Title" },
            DurationMinutes = 15,
            RequiredForCompletion = true,
            RequiresAudio = true,
            RequiresVideo = false,
            OpensInNewTab = true,
            ContentPath = "https://example.com/lesson"
        };

        var result = _mapper.Map<LessonDto>(lesson);

        Assert.NotNull(result);
        Assert.Equal(lesson.LearningObjectId, result.LearningObjectId);
        Assert.Equal(lesson.LessonType, result.LessonType);
        Assert.Equal(lesson.Name.En, result.Name.En);
        Assert.Equal(lesson.DurationMinutes, result.DurationMinutes);
        Assert.Equal(lesson.RequiredForCompletion, result.RequiredForCompletion);
        Assert.Equal(lesson.RequiresAudio, result.RequiresAudio);
        Assert.Equal(lesson.RequiresVideo, result.RequiresVideo);
        Assert.Equal(lesson.OpensInNewTab, result.OpensInNewTab);
        Assert.Equal(lesson.ContentPath, result.ContentPath);
    }

    [Fact]
    public void Map_LessonDtoToLesson_ShouldMapCorrectly()
    {
        var dto = new LessonDto
        {
            LearningObjectId = Guid.NewGuid(),
            LearningObjectType = 0,
            LessonType = "url",
            Name = new LocalizedStringDto { En = "Lesson Title" },
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = true,
            OpensInNewTab = true,
            ContentPath = "https://example.com/lesson"
        };

        var result = _mapper.Map<Lesson>(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.LearningObjectId, result.LearningObjectId);
        Assert.Equal(dto.LessonType, result.LessonType);
        Assert.Equal(dto.Name.En, result.Name.En);
        Assert.Equal(dto.DurationMinutes, result.DurationMinutes);
        Assert.Equal(dto.RequiredForCompletion, result.RequiredForCompletion);
        Assert.Equal(dto.RequiresAudio, result.RequiresAudio);
        Assert.Equal(dto.RequiresVideo, result.RequiresVideo);
        Assert.Equal(dto.OpensInNewTab, result.OpensInNewTab);
        Assert.Equal(dto.ContentPath, result.ContentPath);
    }

    [Fact]
    public void Map_SectionUpdateDtoToLearningContentSection_ShouldMapCorrectly()
    {
        var dto = new SectionUpdateDto
        {
            Action = PatchAction.Replace,
            SectionId = Guid.NewGuid(),
            Name = new LocalizedStringDto { En = "Updated Section" }
        };

        var result = _mapper.Map<LearningContentSection>(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.SectionId, result.SectionId);
        Assert.Equal(dto.Name.En, result.Name.En);
        Assert.Empty(result.LearningObjects);
    }
}
