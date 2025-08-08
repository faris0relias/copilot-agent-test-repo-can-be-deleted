using AutoMapper;
using Moq;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Interfaces.Content;
using Relias.ContentLibraryService.App.Services.Content;
using Relias.ContentLibraryService.Domain.Language;

namespace Relias.ContentLibraryService.App.Tests.Services.Content;

public class LanguageServiceTests
{
    private readonly Mock<ILanguageRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly LanguageService _service;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public LanguageServiceTests()
    {
        _service = new LanguageService(_repositoryMock.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetLanguagesAsync_WhenLanguagesExist_ReturnsListOfLanguageDtos()
    {
        var expectedLanguageId1 = Guid.NewGuid();
        var expectedLanguageId2 = Guid.NewGuid();
        var expectedLanguages = new List<Language>
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

        _repositoryMock
            .Setup(repo => repo.GetLanguagesAsync(_cancellationToken))
            .ReturnsAsync(expectedLanguages);

        _mockMapper
            .Setup(m => m.Map<IEnumerable<LanguageDto>>(It.IsAny<IEnumerable<Language>>()))
            .Returns((IEnumerable<Language> languages) => languages
                .Select(l => new LanguageDto
                {
                    LanguageId = l.LanguageId,
                    Code = l.Code,
                    Name = l.Name
                }));

        var result = await _service.GetLanguagesAsync(_cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, l => l.LanguageId == expectedLanguageId1 && l.Code == "en" && l.Name == "English");
        Assert.Contains(result, l => l.LanguageId == expectedLanguageId2 && l.Code == "es" && l.Name == "Spanish");

        _repositoryMock.Verify(repo => repo.GetLanguagesAsync(_cancellationToken), Times.Once);
    }

}
