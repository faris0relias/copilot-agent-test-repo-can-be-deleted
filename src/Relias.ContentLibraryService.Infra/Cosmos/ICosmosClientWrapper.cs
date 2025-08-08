using Microsoft.Azure.Cosmos;
namespace Relias.ContentLibraryService.Infra.Cosmos;

public interface ICosmosClientWrapper
{
    Task<T> CreateItemAsync<T>(T item, CancellationToken cancellationToken = default);
    Task<T> UpdateItemAsync<T>(T item, string id, CancellationToken cancellationToken = default);
    Task<T> PatchItemAsync<T>(T item, string id, PartitionKey partitionKey, IEnumerable<string> propertiesToPatch, CancellationToken cancellationToken = default);
    Task<T> ReadItemAsync<T>(string id, PartitionKey partitionKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> QueryItemsAsync<T>(Func<IQueryable<T>, IQueryable<T>> queryBuilder, CancellationToken cancellationToken = default);
    Task<T?> QueryFirstOrDefaultAsync<T>(Func<IQueryable<T>, IQueryable<T>> queryBuilder, CancellationToken cancellationToken = default);
    Task<T> DeleteItemAsync<T>(string id, PartitionKey partitionKey, CancellationToken cancellationToken = default);
}
