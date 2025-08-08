using Moq;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Features.Content.Queries;
using Relias.ContentLibraryService.App.Interfaces.Content;

namespace Relias.ContentLibraryService.App.Tests.Features.Content.Queries;

public class GetContentByIdsQueryHandlerTests
{
    private readonly Mock<IContentService> _serviceMock = new();
    private readonly GetContentByIdsQuery.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetContentByIdsQueryHandlerTests()
    {
        _handler = new GetContentByIdsQuery.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenContentExists_ReturnsListOfContentDtos()
    {
        Guid[] contentIds = [Guid.NewGuid(), Guid.NewGuid()];
        var expectedContents = new List<ContentInfoDto>
        {
            new() { ContentTypeId = 1, ContentId = contentIds[0], Title = "Test 1", TimeToCompleteInMinutes = 10 },
            new() { ContentTypeId = 2, ContentId = contentIds[1], Title = "Test 2" }
        };

        _serviceMock
            .Setup(s => s.GetContentByIdsAsync(It.IsAny<IEnumerable<Guid>>(), _cancellationToken))
            .ReturnsAsync(expectedContents);

        var query = new GetContentByIdsQuery.Contract
        {
            ContentIds = contentIds
        };

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.Equal(expectedContents.Count, result.Count());
        Assert.Equivalent(expectedContents, result);
        _serviceMock.Verify(s => s.GetContentByIdsAsync(contentIds, _cancellationToken), Times.Once);
    }
}
