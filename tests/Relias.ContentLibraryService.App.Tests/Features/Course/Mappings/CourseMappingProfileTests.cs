using AutoMapper;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Mappings;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Mappings;

public class CourseMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _mapperConfig;

    public CourseMappingProfileTests()
    {
        _mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<CourseMappingProfile>());
        _mapper = _mapperConfig.CreateMapper();
    }

    [Fact]
    public void CourseMappingProfile_ShouldBeValid()
    {
        _mapperConfig.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_CourseToCourseDto_ShouldMapCorrectly()
    {
        var course = new Domain.Course.Course
        {
            CourseId = Guid.NewGuid(),
            ContentId = Guid.NewGuid(),
            OrganizationId = 1,
            ContentCode = "C101",
            Title = "Test Course",
            BriefDescription = "Brief Description",
            Description = "Full Description",
            StatusId = 2,
            Created = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid().ToString(),
            LastModified = DateTime.UtcNow.AddDays(-1),
            LastModifiedBy = Guid.NewGuid().ToString()
        };

        var result = _mapper.Map<CourseDto>(course);

        Assert.NotNull(result);
        Assert.Equal(course.CourseId, result.CourseId);
        Assert.Equal(course.ContentId, result.ContentId);
        Assert.Equal(course.OrganizationId, result.OrganizationId);
        Assert.Equal(course.ContentCode, result.ContentCode);
        Assert.Equal(course.Title, result.Title);
        Assert.Equal(course.BriefDescription, result.BriefDescription);
        Assert.Equal(course.Description, result.Description);
        Assert.Equal(course.StatusId, result.StatusId);
        Assert.Equal(course.Created, result.Created);
        Assert.Equal(course.CreatedBy, result.CreatedBy);
        Assert.Equal(course.LastModified, result.LastModified);
        Assert.Equal(course.LastModifiedBy, result.LastModifiedBy);
    }

    [Fact]
    public void Map_CourseWithReliasCoursePropertiesToCourseDto_ShouldMapReliasCoursePropertiesCorrectly()
    {
        var course = new Domain.Course.Course
        {
            CourseId = Guid.NewGuid(),
            ContentId = Guid.NewGuid(),
            OrganizationId = 1,
            ContentCode = "C101",
            Title = "Test Course",
            StatusId = 2,
            Created = DateTime.UtcNow,
            IsRelias = true,
            ReliasCourseProperties = new ReliasCourseProperties
            {
                Outline = "Course Outline",
                Disclosure = "Some disclosure",
                CommercialProductDisclaimer = new CommercialProductDisclaimer { Disclaimer = "CP Disclaimer" },
                CompletionRequirement = new CompletionRequirement { Requirement = "Completion rule" },
                ContentDisclaimer = new ContentDisclaimer { Disclaimer = "Content warning" },
                CulturalAwarenessStatement = new CulturalAwarenessStatement { Statement = "Cultural note" },
                RequestForAccommodations = new RequestForAccommodations { Request = "Accommodation info" },
                LearningObjectives =
                [
                    new LearningObjective { Objective = "Objective A" },
                    new LearningObjective { Objective = "Objective B" }
                ],
                Contributors =
                [
                    new CourseContributor
                    {
                        Contributor = new Contributor
                        {
                            Created = DateTime.UtcNow,
                            ContributorId = 42,
                            NameAndCredentials = "Dr. Jane Smith",
                            ShortBio = "Short bio",
                            DisclosureStatement = "No conflict"
                        }
                    }
                ],
                CareSettings =
                [
                    new() { CareSetting = new CareSetting { Setting = "Hospital" } }
                ],
                TargetAudiences =
                [
                    new() { TargetAudience = new TargetAudience { Audience = "Clinicians" } }
                ],
                    TrainingTopics =
                [
                    new() { TrainingTopic = new TrainingTopic { Topic = "HIPAA" } }
                ],
                    Disclosures =
                [
                    new() { Disclosure = new Disclosure { Statement = "General Disclosure" } }
                ]
            }
        };

        var result = _mapper.Map<CourseDto>(course);

        Assert.NotNull(result.ReliasCourseProperties);
        var props = result.ReliasCourseProperties!;

        Assert.Equal("Course Outline", props.Outline);
        Assert.Equal("Some disclosure", props.Disclosure);
        Assert.Equal("CP Disclaimer", props.CommercialProductDisclaimer);
        Assert.Equal("Completion rule", props.CompletionRequirement);
        Assert.Equal("Content warning", props.ContentDisclaimer);
        Assert.Equal("Cultural note", props.CulturalAwarenessStatement);
        Assert.Equal("Accommodation info", props.RequestForAccommodations);

        Assert.Equal(2, props.LearningObjectives.Count);
        Assert.Contains("Objective A", props.LearningObjectives);
        Assert.Contains("Objective B", props.LearningObjectives);

        Assert.Single(props.Contributors);
        var contributor = props.Contributors.First();
        Assert.Equal(42, contributor.ContributorIdNum);
        Assert.Equal("Dr. Jane Smith", contributor.NameAndCredentials);
        Assert.Equal("Short bio", contributor.ShortBio);
        Assert.Equal("No conflict", contributor.DisclosureStatement);

        Assert.Single(props.CareSettings);
        Assert.Equal("Hospital", props.CareSettings.First());

        Assert.Single(props.TargetAudiences);
        Assert.Equal("Clinicians", props.TargetAudiences.First());

        Assert.Single(props.TrainingTopics);
        Assert.Equal("HIPAA", props.TrainingTopics.First());

        Assert.Single(props.Disclosures);
        Assert.Equal("General Disclosure", props.Disclosures.First());
    }

    [Fact]
    public void Map_CourseDtoToCourse_ShouldMapCorrectly()
    {
        var dto = new CourseDto
        {
            CourseId = Guid.NewGuid(),
            ContentId = Guid.NewGuid(),
            OrganizationId = 1,
            ContentCode = "C101",
            Title = "Test Course",
            BriefDescription = "Brief Description",
            Description = "Full Description",
            StatusId = 2,
            Created = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid().ToString(),
            LastModified = DateTime.UtcNow.AddDays(-1),
            LastModifiedBy = Guid.NewGuid().ToString()
        };

        var result = _mapper.Map<Domain.Course.Course>(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.CourseId, result.CourseId);
        Assert.Equal(dto.ContentId, result.ContentId);
        Assert.Equal(dto.OrganizationId, result.OrganizationId);
        Assert.Equal(dto.ContentCode, result.ContentCode);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.BriefDescription, result.BriefDescription);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.StatusId, result.StatusId);
        Assert.Equal(dto.Created, result.Created);
        Assert.Equal(dto.CreatedBy.ToString(), result.CreatedBy);
        Assert.Equal(dto.LastModified, result.LastModified);
        Assert.Equal(dto.LastModifiedBy.ToString(), result.LastModifiedBy);
    }
}
