using AutoMapper;
using Moq;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Interfaces.Content;
using Relias.ContentLibraryService.App.Services.Content;
using Relias.ContentLibraryService.Domain.ContentType;

namespace Relias.ContentLibraryService.App.Tests.Services.Content;

public class ContentTypeServiceTests
{
    private readonly Mock<IContentTypeRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ContentTypeService _service;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public ContentTypeServiceTests()
    {
        _service = new ContentTypeService(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetContentTypesAsync_WhenContentTypesExist_ReturnsListOfContentTypeDtos()
    {
        var expectedContentTypes = new List<ContentType>
            {
                new() { ContentTypeId = 1, ContentTypeDescription = "Content Type 1" },
                new() { ContentTypeId = 2, ContentTypeDescription = "Content Type 2" }
            };

        _repositoryMock
            .Setup(repo => repo.GetContentTypesAsync(_cancellationToken))
            .ReturnsAsync(expectedContentTypes);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<ContentTypeDto>>(It.IsAny<IEnumerable<ContentType>>()))
            .Returns((IEnumerable<ContentType> languages) => languages
                .Select(ct => new ContentTypeDto
                {
                    ContentTypeId = ct.ContentTypeId,
                    ContentTypeDescription = ct.ContentTypeDescription
                }));

        var result = await _service.GetContentTypesAsync(_cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, ct => ct.ContentTypeId == 1 && ct.ContentTypeDescription == "Content Type 1");
        Assert.Contains(result, ct => ct.ContentTypeId == 2 && ct.ContentTypeDescription == "Content Type 2");

        _repositoryMock.Verify(repo => repo.GetContentTypesAsync(_cancellationToken), Times.Once);
    }
}
