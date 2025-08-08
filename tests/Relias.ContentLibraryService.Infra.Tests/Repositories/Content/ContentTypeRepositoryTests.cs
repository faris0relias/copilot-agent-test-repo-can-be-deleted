using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Relias.ContentLibraryService.Domain.ContentType;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Infra.Repositories;

namespace Relias.ContentLibraryService.Infra.Tests.Repositories.Content;

public class ContentTypeRepositoryTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ContentTypeRepository _repository;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public ContentTypeRepositoryTests()
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
        _repository = new ContentTypeRepository(_dbContext);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _dbContext.ContentType.AddRange(new List<ContentType>
        {
            new() { ContentTypeId = 1, ContentTypeDescription = "Content Type 1" },
            new() { ContentTypeId = 2, ContentTypeDescription = "Content Type 2" }
        });

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetContentTypesAsync_WhenContentTypesExist_ReturnsListOfContentType()
    {
        var result = await _repository.GetContentTypesAsync(_cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, ct => ct.ContentTypeId == 1 && ct.ContentTypeDescription == "Content Type 1");
        Assert.Contains(result, ct => ct.ContentTypeId == 2 && ct.ContentTypeDescription == "Content Type 2");
    }
}
