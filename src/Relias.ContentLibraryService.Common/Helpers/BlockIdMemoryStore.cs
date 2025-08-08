using System.Collections.Concurrent;

namespace Relias.ContentLibraryService.Common.Helpers;

public static class BlockIdMemoryStore
{
    private static readonly ConcurrentDictionary<string, UploadState> _store = new();

    public static bool Add(string uploadId, int index, string blockId, int totalChunks)
    {
        var uploadState = _store.GetOrAdd(uploadId, _ => new UploadState(totalChunks));
        uploadState.BlockIds[index] = blockId;
        
        return uploadState.IsComplete();
    }

    public static List<string> Get(string uploadId)
    {
        return _store.TryGetValue(uploadId, out var uploadState)
            ? uploadState.BlockIds.Where(id => !string.IsNullOrEmpty(id)).ToList()
            : new List<string>();
    }

    public static void Remove(string uploadId)
    {
        _store.TryRemove(uploadId, out _);
    }

    private class UploadState
    {
        public string[] BlockIds { get; }
        public int TotalChunks { get; }

        public UploadState(int totalChunks)
        {
            TotalChunks = totalChunks;
            BlockIds = new string[totalChunks];
        }

        public bool IsComplete() => BlockIds.All(id => !string.IsNullOrEmpty(id));
    }
}