using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Common.Services;
using Relias.ContentLibraryService.Domain.ContentType;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Infra.Repositories;

namespace Relias.ContentLibraryService.Infra.Tests.Repositories;

public class CourseRepositoryTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDomainEventService> _domainEventServiceMock;
    private readonly FakeTimeProvider _timeProvider;

    private readonly ApplicationDbContext _dbContext;
    private readonly FakeLogger<CourseRepository> _logger;
    private readonly CourseRepository _repository;

    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private readonly ContentType _courseContentType = new() { ContentTypeId = 2, ContentTypeDescription = "Course" };
    private readonly CreateCourseDto _validNewCourse = new()
    {
        OrganizationId = ValidOrgId,
        CourseName = ValidCourseName,
        ContentCode = ValidContentCode
    };

    private const int ValidOrgId = 1;
    private const string ValidCourseName = "Valid Course";
    private const string ValidContentCode = "VALID-COURSE-CODE";
    private static readonly Guid ContentId = Guid.Parse("a3c6b2e9-7b6d-4c33-9234-c2f9a3c4b3d9");
    private static readonly Guid CourseId = Guid.Parse("b5d7b1f8-8b5c-4e44-9353-b3f8a1d5a1f8");

    private readonly Course _courseWithKnownGuids = new()
    {
        CourseId = CourseId,
        ContentId = ContentId,
        OrganizationId = 123,
        ContentCode = "ABC101",
        Title = "Test Course",
        BriefDescription = "Brief Test",
        StatusId = 1,
        Created = DateTime.UtcNow,
        CreatedBy = "11111111-1111-1111-1111-111111111111"
    };

    private readonly Domain.Content.Content _contentWithKnownGuids = new()
    {
        ContentId = ContentId
    };

    public CourseRepositoryTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _domainEventServiceMock = new Mock<IDomainEventService>();
        _timeProvider = new FakeTimeProvider();
        _logger = new FakeLogger<CourseRepository>();

        // Configure in-memory database
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "UseInMemoryDatabase", "true" }
            })
            .Build();

        var services = new ServiceCollection();
        services
            .AddScoped<ICurrentUserService>(_ => _currentUserServiceMock.Object)
            .AddScoped<IDomainEventService>(_ => _domainEventServiceMock.Object)
            .AddScoped<TimeProvider>(_ => _timeProvider);

        var provider = services
            .AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase($"ReliasDb_{Guid.NewGuid()}");
                options.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            })
            .BuildServiceProvider();

        _dbContext = provider.GetRequiredService<ApplicationDbContext>();
        _repository = new CourseRepository(_logger, _dbContext);

        SeedDatabase();
    }

    [Fact]
    public async Task GetCoursesAsync_Should_Return_EmptyList_If_OrgId_Not_Exist()
    {
        var request = new GetCoursesQuery.Contract
        {
            OrganizationId = 1
        };

        var result = await _repository.GetCoursesAsync(request.OrganizationId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCoursesAsync_Should_Return_Course_List_If_OrgId_Exist()
    {
        var request = new GetCoursesQuery.Contract
        {
            OrganizationId = 8
        };

        var resultEnumerable = await _repository.GetCoursesAsync(request.OrganizationId, CancellationToken.None);
        var result = resultEnumerable.ToList();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Single(result);

    }

    [Fact]
    public async Task CreateCourseAsync_WhenDuplicateContentCodeProvided_ThrowsBadRequestException()
    {
        var currentUserId = Guid.NewGuid();
        var currentDateTime = new DateTimeOffset(2000, 01, 01, 12, 00, 00, TimeSpan.FromHours(0));
        _currentUserServiceMock
            .Setup(cus => cus.UserId)
            .Returns(currentUserId.ToString());

        _timeProvider.SetLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        _timeProvider.SetUtcNow(currentDateTime);

        var existingContentCode = "EXISTING-COURSE-CODE";
        Domain.Content.Content existingContent = new()
        {
            ContentId = Guid.NewGuid(),
            ContentTypeId = _courseContentType.ContentTypeId,
            ContentType = _courseContentType
        };
        Course existingCourse = new()
        {
            CourseId = Guid.NewGuid(),
            ContentId = existingContent.ContentId,
            OrganizationId = 1,
            ContentCode = existingContentCode,
            Title = "Existing Course",
            StatusId = 1,
            Created = DateTime.Now,
            CreatedBy = Guid.NewGuid().ToString()
        };

        await _dbContext.Content.AddAsync(existingContent, _cancellationToken);
        await _dbContext.Courses.AddAsync(existingCourse, _cancellationToken);

        await _dbContext.SaveChangesAsync(_cancellationToken);

        CreateCourseDto courseWithDuplicateContentCode = new()
        {
            OrganizationId = ValidOrgId,
            CourseName = ValidCourseName,
            ContentCode = existingContentCode
        };

        await Assert.ThrowsAsync<BadRequestException>(() => _repository.CreateCourseAsync(courseWithDuplicateContentCode, _cancellationToken));

        var debugLog = _logger.LatestRecord;
        Assert.NotNull(debugLog);
        var courseId = debugLog.StructuredState?.SingleOrDefault(x => x.Key == "CourseId").Value;
        Assert.NotNull(courseId);

        Assert.Equal(LogLevel.Debug, debugLog.Level);
        Assert.Equal($"Unable to create new course with CourseId {courseId}, content code already exists.", debugLog.Message);
    }

    [Fact]
    public async Task CreateCourseAsync_WhenCourseContentTypeDoesNotExist_ThrowsNotFoundException()
    {
        _dbContext.ContentType.Remove(_courseContentType);
        await _dbContext.SaveChangesAsync(_cancellationToken);
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.CreateCourseAsync(_validNewCourse, _cancellationToken));

        var logs = _logger.Collector.GetSnapshot();
        Assert.NotEmpty(logs);

        var debugLog = logs[0];
        Assert.NotNull(debugLog);
        var courseId = debugLog.StructuredState?.SingleOrDefault(x => x.Key == "CourseId").Value;
        Assert.NotNull(courseId);

        Assert.Equal(LogLevel.Debug, debugLog.Level);
        Assert.Equal($"Unable to create new course with CourseId {courseId}, content type 'Course' not found.", debugLog.Message);

        var errorLog = logs[^1];
        Assert.NotNull(errorLog);

        Assert.NotNull(errorLog.Exception);
        Assert.Equal("ContentType Course not found.", errorLog.Exception.Message);

        courseId = errorLog.StructuredState?.SingleOrDefault(x => x.Key == "CourseId").Value;
        Assert.NotNull(courseId);

        Assert.Equal($"Something went wrong trying to create Course with Course ID {courseId}.", errorLog.Message);
    }

    [Fact]
    public async Task CreateCourseAsync_WhenValidNewCourseProvided_ReturnsCreatedCourseEntity()
    {
        var currentUserId = Guid.NewGuid();
        var currentDateTime = new DateTimeOffset(2000, 01, 01, 12, 00, 00, TimeSpan.FromHours(0));
        _currentUserServiceMock
            .Setup(cus => cus.UserId)
            .Returns(currentUserId.ToString());

        _timeProvider.SetLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        _timeProvider.SetUtcNow(currentDateTime);

        var result = await _repository.CreateCourseAsync(_validNewCourse, _cancellationToken);

        Assert.NotNull(result);
        Assert.IsType<Guid>(result.CourseId);
        Assert.False(result.CourseId.Equals(Guid.Empty));
        Assert.IsType<Guid>(result.ContentId);
        Assert.False(result.ContentId.Equals(Guid.Empty));
        Assert.Equal(1, result.OrganizationId);
        Assert.Equal("VALID-COURSE-CODE", result.ContentCode);
        Assert.Equal("Valid Course", result.Title);
        Assert.Null(result.Description);
        Assert.Empty(result.BriefDescription!);
        Assert.Empty(result.LanguageIds!);
        Assert.Equal(1, result.StatusId);
        Assert.IsType<DateTime>(result.Created);
        Assert.Equal(new DateTime(2000, 01, 01, 07, 00, 00), result.Created);
        Assert.False(result.CreatedBy.IsNullOrEmpty());
        Assert.True(result.CreatedBy?.Equals(currentUserId.ToString()));
        Assert.Null(result.LastModified);
        Assert.Null(result.LastModifiedBy);
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_UpdateCourseProvided_UpdatesPersistedEntity()
    {
        var courseId = Guid.NewGuid();
        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Old Title",
            ContentId = Guid.NewGuid(),
            OrganizationId = 8,
            ContentCode = "OLD-CODE",
            Description = "Old Description",
            BriefDescription = "Old Brief",
            LanguageIds = [Guid.NewGuid()],
            Created = DateTime.UtcNow
        };

        await _dbContext.Courses.AddAsync(existingCourse);
        await _dbContext.SaveChangesAsync();

        existingCourse.Title = "New Title";
        existingCourse.ContentCode = "NEW-CODE";
        existingCourse.Description = "New Description";

        await _repository.UpdateAsync(existingCourse, _cancellationToken);

        var updatedCourse = await _dbContext.Courses.FindAsync(courseId);
        Assert.NotNull(updatedCourse);
        Assert.Equal("New Title", updatedCourse.Title);
        Assert.Equal("NEW-CODE", updatedCourse.ContentCode);
        Assert.Equal("New Description", updatedCourse.Description);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCourseExists_ReturnsCourse()
    {
        var courseId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var course = await _repository.GetByIdAsync(courseId, _cancellationToken);

        Assert.NotNull(course);
        Assert.Equal(courseId, course.CourseId);
        Assert.Equal("Test Course 1", course.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCourseIsRelias_LoadsReliasCourseProperties()
    {
        var courseId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var course = await _repository.GetByIdAsync(courseId, _cancellationToken);

        Assert.NotNull(course);
        Assert.True(course.IsRelias);
        Assert.NotNull(course.ReliasCourseProperties);

        var props = course.ReliasCourseProperties;

        Assert.Equal("Disclaimer text", props.CommercialProductDisclaimer?.Disclaimer);
        Assert.Equal("Complete all", props.CompletionRequirement?.Requirement);
        Assert.Equal("Content warning", props.ContentDisclaimer?.Disclaimer);
        Assert.Equal("Cultural statement", props.CulturalAwarenessStatement?.Statement);
        Assert.Equal("Request info", props.RequestForAccommodations?.Request);

        Assert.NotNull(props.LearningObjectives);
        Assert.Collection(props.LearningObjectives,
            o => Assert.Equal("Objective 1", o.Objective),
            o => Assert.Equal("Objective 2", o.Objective));

        Assert.NotNull(props.Contributors);
        Assert.Single(props.Contributors);
        var contributor = props.Contributors.First().Contributor;
        Assert.Equal("Dr. Jane Smith", contributor.NameAndCredentials);
        Assert.Equal("Bio", contributor.ShortBio);
        Assert.Equal("No conflicts", contributor.DisclosureStatement);

        Assert.NotNull(props.CareSettings);
        Assert.Single(props.CareSettings);
        Assert.Equal("Inpatient", props.CareSettings.First().CareSetting?.Setting);

        Assert.NotNull(props.TargetAudiences);
        Assert.Single(props.TargetAudiences);
        Assert.Equal("Nurses", props.TargetAudiences.First().TargetAudience?.Audience);

        Assert.NotNull(props.TrainingTopics);
        Assert.Single(props.TrainingTopics);
        Assert.Equal("Compliance", props.TrainingTopics.First().TrainingTopic?.Topic);

        Assert.NotNull(props.Disclosures);
        Assert.Single(props.Disclosures);
        Assert.Equal("Disclosure 1", props.Disclosures.First().Disclosure?.Statement);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCourseDoesNotExist_ReturnsNull()
    {
        var courseId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var course = await _repository.GetByIdAsync(courseId, _cancellationToken);

        Assert.Null(course);
    }

    [Fact]
    public async Task ArchiveAsync_WhenCalled_ThrowsNotImplementedException()
    {
        Course archivedCourse = new()
        {
            CourseId = CourseId,
            ContentId = ContentId,
            OrganizationId = 123,
            Created = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<NotImplementedException>(() => _repository.ArchiveAsync(archivedCourse, _cancellationToken));
    }

    [Fact]
    public async Task DeleteAsync_RemovesCourseAndContent_WhenValid()
    {
        var course = await _dbContext.Courses.FindAsync(_courseWithKnownGuids.CourseId);
        var content = await _dbContext.Content.FindAsync(_contentWithKnownGuids.ContentId);

        await _repository.DeleteAsync(course!, content!, _cancellationToken);

        var deletedCourse = await _dbContext.Courses.FindAsync(course!.CourseId);
        var deletedContent = await _dbContext.Content.FindAsync(content!.ContentId);

        Assert.Null(deletedCourse);
        Assert.Null(deletedContent);
    }

    [Fact]
    public async Task DeleteAsync_WhenExceptionOccurs_RollsBackTransaction()
    {
        // Arrange
        var course = await _dbContext.Courses.FindAsync(_courseWithKnownGuids.CourseId);
        var content = await _dbContext.Content.FindAsync(_contentWithKnownGuids.ContentId);

        var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        var mockDatabase = new Mock<DatabaseFacade>(mockContext.Object);

        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Database update failed", new Exception("Inner exception")));

        var errorRepository = new CourseRepository(_logger, mockContext.Object);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => errorRepository.DeleteAsync(course!, content!, _cancellationToken));

        Assert.Equal("Database update failed", exception.Message);

        mockContext.Verify(c => c.Database.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);

        var stillExistingCourse = await _dbContext.Courses.FindAsync(_courseWithKnownGuids.CourseId);
        Assert.NotNull(stillExistingCourse);
    }

    [Fact]
    public async Task FindByContentCodeAsync_WhenMatchingCourseExists_ReturnsMatchingCourse()
    {
        // Arrange
        var course = await _dbContext.Courses.FindAsync(_courseWithKnownGuids.CourseId);
        var excludedCourse = await _dbContext.Courses.FirstAsync(c => c.CourseId != course!.CourseId);

        // Act
        var result = await _repository.FindByContentCodeAsync(course!.ContentCode, excludedCourse.CourseId, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(course.CourseId, result.CourseId);
        Assert.Equal(course.ContentCode, result.ContentCode);
    }

    [Fact]
    public async Task GetContentByCourseAsync_WhenMatchingCourseExists_ReturnsMatchingContent()
    {
        var course = await _dbContext.Courses.FindAsync(_courseWithKnownGuids.CourseId);
        var content = await _dbContext.Content.FindAsync(_contentWithKnownGuids.ContentId);

        var result = await _repository.GetContentByCourseAsync(course!.ContentId!.Value, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(content!.ContentId, result.ContentId);
    }

    [Fact]
    public async Task GetCoursesByContentIdsAsync_WhenMatchingCourseExists_ReturnsMatchingContent()
    {
        var course = await _dbContext.Courses.FindAsync(_courseWithKnownGuids.CourseId);

        var result = await _repository.GetCoursesByContentIdsAsync([course!.ContentId!.Value], _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(course!.CourseId, result.First().CourseId);
    }

    [Fact]
    public async Task TriggerCourseContentUpdateAsync_WhenCourseExists_CallsUpdateAsync()
    {
        var course = _courseWithKnownGuids;
        string userId = Guid.NewGuid().ToString();

        _currentUserServiceMock
            .Setup(cus => cus.UserId)
            .Returns(userId);

        await _repository.TriggerCourseContentUpdateAsync(course.CourseId, _cancellationToken);

        var updatedCourse = await _dbContext.Courses.FindAsync(course.CourseId);

        Assert.NotNull(updatedCourse);
        Assert.Equal(course.LastModifiedBy, userId.ToString());

    }

    [Fact]
    public async Task TriggerCourseContentUpdateAsync_WhenCourseDoesNotExists_ThrowsError()
    {
        var courseId = Guid.NewGuid();
        string userId = Guid.NewGuid().ToString();

        _currentUserServiceMock
            .Setup(cus => cus.UserId)
            .Returns(userId);

        var result = await Assert.ThrowsAsync<NotFoundException>(
            () => _repository.TriggerCourseContentUpdateAsync(courseId, _cancellationToken));

        Assert.Equal(
            $"Failed to update course metadata: Course with ID {courseId} not found.",
            result.Message);

    }


    private void SeedDatabase()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
        _dbContext.ContentType.Add(_courseContentType);

        _dbContext.Courses.AddRange(new List<Course>
        {
            new ()
            {
                CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ContentId = Guid.NewGuid(),
                OrganizationId = 8,
                ContentCode = "C101",
                Title = "Test Course 1",
                BriefDescription = "Brief 1",
                StatusId = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "11111111-1111-1111-1111-111111111111"
            },
            new ()
            {
                CourseId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ContentId = Guid.NewGuid(),
                OrganizationId = 9,
                ContentCode = "C102",
                Title = "Test Course 2",
                BriefDescription = "Brief 2",
                StatusId = 2,
                Created = DateTime.UtcNow,
                CreatedBy = "22222222-2222-2222-2222-222222222222"
            },
            new ()
            {
                CourseId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                ContentId = Guid.NewGuid(),
                OrganizationId = 9,
                ContentCode = "C102",
                Title = "Test Course 2",
                BriefDescription = "Brief 2",
                StatusId = 2,
                Created = DateTime.UtcNow,
                CreatedBy = "33333333-3333-3333-3333-333333333333",
                IsRelias = true,
                ReliasCourseProperties = new ReliasCourseProperties
                {
                    CommercialProductDisclaimer = new CommercialProductDisclaimer { Disclaimer = "Disclaimer text" },
                    CompletionRequirement = new CompletionRequirement { Requirement = "Complete all" },
                    ContentDisclaimer = new ContentDisclaimer { Disclaimer = "Content warning" },
                    CulturalAwarenessStatement = new CulturalAwarenessStatement { Statement = "Cultural statement" },
                    RequestForAccommodations = new RequestForAccommodations { Request = "Request info" },
                    LearningObjectives =
                    [
                        new() { Objective = "Objective 1" },
                        new() { Objective = "Objective 2" }
                    ],
                    Contributors =
                    [
                        new()
                        {
                            Contributor = new Contributor
                            {
                                Created = DateTime.UtcNow,
                                NameAndCredentials = "Dr. Jane Smith",
                                ShortBio = "Bio",
                                DisclosureStatement = "No conflicts"
                            }
                        }
                    ],
                    CareSettings =
                    [
                        new() { CareSetting = new CareSetting { Setting = "Inpatient" } }
                    ],
                    TargetAudiences =
                    [
                        new() { TargetAudience = new TargetAudience { Audience = "Nurses" } }
                    ],
                    TrainingTopics =
                    [
                        new() { TrainingTopic = new TrainingTopic { Topic = "Compliance" } }
                    ],
                    Disclosures =
                    [
                        new() { Disclosure = new Disclosure { Statement = "Disclosure 1" } }
                    ]
                }
            },
            _courseWithKnownGuids
        });

        _dbContext.Content.Add(new Domain.Content.Content { ContentId = _contentWithKnownGuids.ContentId });

        _dbContext.SaveChanges();
    }
}
