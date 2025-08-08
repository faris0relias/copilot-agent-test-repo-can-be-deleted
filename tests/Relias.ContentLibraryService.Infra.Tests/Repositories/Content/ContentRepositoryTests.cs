using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Infra.Repositories;

namespace Relias.ContentLibraryService.Infra.Tests.Repositories.Content;

public class ContentRepositoryTests
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly ApplicationDbContext _dbContext;
    private readonly ContentRepository _repository;

    public ContentRepositoryTests()
    {
        var services = new ServiceCollection();

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
        _repository = new ContentRepository(_dbContext);
    }

    [Fact]
    public async Task GetContentByContentIds_WhenMatchingContentExists_ReturnsCorrectContent()
    {
        Guid[] contentIds = [Guid.NewGuid(), Guid.NewGuid()];

        var contents = new List<Domain.Content.Content>
        {
            new() { ContentId = contentIds[0], ContentTypeId = 1 },
            new() { ContentId = contentIds[1], ContentTypeId = 2 }
        };

        await _dbContext.Content.AddRangeAsync(contents);
        await _dbContext.Content.AddAsync(new Domain.Content.Content { ContentId = Guid.NewGuid(), ContentTypeId = 1 });
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetContentByContentIdsAsync(contentIds, _cancellationToken);

        Assert.Equivalent(contents, result);
    }
}
