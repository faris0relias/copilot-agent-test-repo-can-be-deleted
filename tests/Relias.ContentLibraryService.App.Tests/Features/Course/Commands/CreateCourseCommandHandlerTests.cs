using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Commands;

public class CreateCourseCommandHandlerTests
{
    private readonly Mock<ICourseService> _serviceMock = new();
    private readonly CreateCourseCommand.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private static readonly CreateCourseDto ValidNewCourse = new()
    {
        OrganizationId = 1,
        CourseName = "Valid Course",
        ContentCode = "VALID-COURSE-CODE"
    };

    private static readonly Guid ValidCourseId = Guid.NewGuid();
    private static readonly Guid ValidContentId = Guid.NewGuid();
    private static readonly string ValidCreatedById = Guid.NewGuid().ToString();

    private static readonly CourseDto ValidCreatedCourse = new()
    {
        CourseId = ValidCourseId,
        ContentId = ValidContentId,
        OrganizationId = 1,
        ContentCode = ValidNewCourse.ContentCode,
        Title = ValidNewCourse.CourseName,
        StatusId = 1,
        Created = DateTime.Now,
        CreatedBy = ValidCreatedById
    };

    public CreateCourseCommandHandlerTests()
    {
        _handler = new CreateCourseCommand.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidNewCourseProvided_ReturnsCourseDto()
    {
        _serviceMock
            .Setup(cs => cs.CreateCourseAsync(ValidNewCourse, _cancellationToken))
            .ReturnsAsync(ValidCreatedCourse);

        CreateCourseCommand.Contract command = new() { NewCourse = ValidNewCourse };

        var result = await _handler.Handle(command, _cancellationToken);

        Assert.NotNull(result);
        Assert.True(result.CourseId.Equals(ValidCourseId));
        Assert.True(result.ContentId.Equals(ValidContentId));
        Assert.Equal(1,result.OrganizationId);
        Assert.Equal("VALID-COURSE-CODE",result.ContentCode);
        Assert.Equal("Valid Course", result.Title);
        Assert.Null(result.Description);
        Assert.Null(result.BriefDescription);
        Assert.Empty(result.LanguageIds);
        Assert.Equal(1,result.StatusId);
        Assert.IsType<DateTime>(result.Created);
        Assert.NotNull(result.CreatedBy);
        Assert.Null(result.LastModified);
        Assert.Null(result.LastModifiedBy);

        _serviceMock.Verify(cs => cs.CreateCourseAsync(ValidNewCourse, _cancellationToken), Times.Once);
    }
}