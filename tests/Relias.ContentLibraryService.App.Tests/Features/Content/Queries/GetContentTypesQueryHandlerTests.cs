using Moq;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Features.Content.Queries;
using Relias.ContentLibraryService.App.Interfaces.Content;

namespace Relias.ContentLibraryService.App.Tests.Features.Content.Queries;

public class GetContentTypesQueryHandlerTests
{
    private readonly Mock<IContentTypeService> _serviceMock = new ();
    private readonly GetContentTypesQuery.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetContentTypesQueryHandlerTests()
    {
        _handler = new GetContentTypesQuery.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenContentTypesExist_ReturnsListOfContentTypeDtos()
    {
        var expectedContentTypes = new List<ContentTypeDto>
        {
            new() { ContentTypeId = 1, ContentTypeDescription = "Content Type 1" },
            new() { ContentTypeId = 2, ContentTypeDescription = "Content Type 2" }
        };

        _serviceMock
            .Setup(repo => repo.GetContentTypesAsync(_cancellationToken))
            .ReturnsAsync(expectedContentTypes);

        var query = new GetContentTypesQuery.Contract();

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, ct => ct.ContentTypeId == 1 && ct.ContentTypeDescription == "Content Type 1");
        Assert.Contains(result, ct => ct.ContentTypeId == 2 && ct.ContentTypeDescription == "Content Type 2");

        _serviceMock.Verify(repo => repo.GetContentTypesAsync(_cancellationToken), Times.Once);
    }
}
