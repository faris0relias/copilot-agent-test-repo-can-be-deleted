using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Relias.ContentLibraryService.Domain.Language;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Infra.Repositories;

namespace Relias.ContentLibraryService.Infra.Tests.Repositories.Content;

public class LanguageRepositoryTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly LanguageRepository _repository;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly Guid LanguageId1 = Guid.NewGuid();
    private readonly Guid LanguageId2 = Guid.NewGuid();

    public LanguageRepositoryTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "UseInMemoryDatabase", "true" }
            })
            .Build();

        var services = new ServiceCollection();
        var provider = services
            .AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase($"ReliasDb_{Guid.NewGuid()}"))
            .BuildServiceProvider();

        _dbContext = provider.GetRequiredService<ApplicationDbContext>();
        _repository = new LanguageRepository(_dbContext);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _dbContext.Languages.AddRange(new List<Language>
        {
            new()
            {
                LanguageId = LanguageId1,
                Code = "en",
                Name = "English"
            },
            new()
            {
                LanguageId = LanguageId2,
                Code = "es",
                Name = "Spanish"
            }
        });

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetLanguagesAsync_WhenLanguagesExist_ReturnsListOfLanguages()
    {
        var result = await _repository.GetLanguagesAsync(_cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, l => l.LanguageId == LanguageId1 && l.Code == "en" && l.Name == "English");
        Assert.Contains(result, l => l.LanguageId == LanguageId2 && l.Code == "es" && l.Name == "Spanish");
    }
}
