using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Net;

namespace Relias.ContentLibraryService.Infra.Cosmos;

public class CosmosClientWrapper(CosmosClient client, string databaseId, CosmosLinqSerializerOptions linqSerializerOptions) : ICosmosClientWrapper
{
    private readonly CosmosClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly string _databaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));

    public async Task<T> CreateItemAsync<T>(T item, CancellationToken cancellationToken = default)
    {
        var container = GetContainer(typeof(T).Name);
        var response = await container.CreateItemAsync(item, cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(
    Func<IQueryable<T>, IQueryable<T>> queryBuilder,
    CancellationToken cancellationToken = default)
    {
        var container = GetContainer(typeof(T).Name);
        var queryable = container.GetItemLinqQueryable<T>(
            allowSynchronousQueryExecution: false,
            requestOptions: new QueryRequestOptions { MaxItemCount = 1 },
            linqSerializerOptions: linqSerializerOptions);

        var query = queryBuilder(queryable);
        var iterator = query.ToFeedIterator();

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            return response.Resource.FirstOrDefault();
        }

        return default;
    }

    public async Task<IEnumerable<T>> QueryItemsAsync<T>(Func<IQueryable<T>, IQueryable<T>> queryBuilder, CancellationToken cancellationToken = default)
    {
        var container = GetContainer(typeof(T).Name);
        var queryable = container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution: false);
        var query = queryBuilder(queryable);
        var iterator = query.ToFeedIterator();
        var results = new List<T>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response.Resource);
        }

        return results;
    }

    public async Task<T> ReadItemAsync<T>(string id, PartitionKey partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var container = GetContainer(typeof(T).Name);

            var response = await container.ReadItemAsync<T>(
                id,
                partitionKey,
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(id);
        }
    }

    public async Task<T> UpdateItemAsync<T>(T item, string id, CancellationToken cancellationToken = default)
    {
        var container = GetContainer(typeof(T).Name);
        var response = await container.ReplaceItemAsync(item, id, cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task<T> PatchItemAsync<T>(T item, string id, PartitionKey partitionKey, IEnumerable<string> propertiesToPatch, CancellationToken cancellationToken = default)
    {
        var container = GetContainer(typeof(T).Name);

        var operations = item?.GetType()
            .GetProperties()
            .Where(propertyInfo => propertiesToPatch.Contains(propertyInfo.Name))
            .Select(propertyInfo => PatchOperation.Set($"/{char.ToLowerInvariant(propertyInfo.Name[0]) + propertyInfo.Name[1..]}", propertyInfo.GetValue(item)))
            .ToList();

        var response = await container.PatchItemAsync<T>(
            id,
            partitionKey,
            operations,
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    private Container GetContainer(string containerName)
    {
        return _client.GetContainer(_databaseId, containerName);
    }

    public async Task<T> DeleteItemAsync<T>(string id, PartitionKey partitionKey, CancellationToken cancellationToken = default)
    {
        var container = GetContainer(typeof (T).Name);
        var response = await container.DeleteItemAsync<T>(id,partitionKey, cancellationToken:cancellationToken);

        return response.Resource;
    }
}
