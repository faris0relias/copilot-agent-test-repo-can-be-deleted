using Moq;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Queries;

public class GetCoursesQueryHandlerTests
{
    private readonly Mock<ICourseService> _serviceMock = new();
    private readonly GetCoursesQuery.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetCoursesQueryHandlerTests()
    {
        _handler = new GetCoursesQuery.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCoursesExist_ReturnsListOfCourseDtos()
    {
        var expectedCourses = new List<CourseDto>
        {
            new() { Created = DateTime.Now, Title = "Ttitle_1", BriefDescription = "Brief_1", ContentCode = "Code_1",  CourseId = Guid.NewGuid(), ContentId = Guid.NewGuid(), OrganizationId = 123  },
            new() { Created = DateTime.Now, Title = "Ttitle_2", BriefDescription = "Brief_2", ContentCode = "Code_2", CourseId = Guid.NewGuid(), ContentId = Guid.NewGuid(),  OrganizationId = 123 }
        };

        _serviceMock
            .Setup(repo => repo.GetCoursesAsync(123, _cancellationToken))
            .ReturnsAsync(expectedCourses);

        var query = new GetCoursesQuery.Contract
        {
            OrganizationId = 123
        };

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, co => co.OrganizationId == 123 && co.Title == "Ttitle_1");

        _serviceMock.Verify(repo => repo.GetCoursesAsync(123, _cancellationToken), Times.Once);
    }
}
