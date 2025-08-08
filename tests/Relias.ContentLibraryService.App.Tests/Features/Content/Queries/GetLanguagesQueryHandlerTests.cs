using Moq;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Features.Content.Queries;
using Relias.ContentLibraryService.App.Interfaces.Content;

namespace Relias.ContentLibraryService.App.Tests.Features.Content.Queries;

public class GetLanguagesQueryHandlerTests
{
    private readonly Mock<ILanguageService> _serviceMock = new();
    private readonly GetLanguagesQuery.Handler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetLanguagesQueryHandlerTests()
    {
        _handler = new GetLanguagesQuery.Handler(_serviceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenLanguagesExist_ReturnsListOfLanguageDtos()
    {
        var expectedLanguageId1 = Guid.NewGuid();
        var expectedLanguageId2 = Guid.NewGuid();
        var expectedLanguages = new List<LanguageDto>
        {
            new()
            {
                LanguageId = expectedLanguageId1,
                Code = "en",
                Name = "English"
            },
            new()
            {
                LanguageId = expectedLanguageId2,
                Code = "es",
                Name = "Spanish"
            }
        };

        _serviceMock
            .Setup(repo => repo.GetLanguagesAsync(_cancellationToken))
            .ReturnsAsync(expectedLanguages);

        var query = new GetLanguagesQuery.Contract();

        var result = await _handler.Handle(query, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, l => l.LanguageId == expectedLanguageId1 && l.Code == "en" && l.Name == "English");
        Assert.Contains(result, l => l.LanguageId == expectedLanguageId2 && l.Code == "es" && l.Name == "Spanish");

        _serviceMock.Verify(repo => repo.GetLanguagesAsync(_cancellationToken), Times.Once);
    }
}

