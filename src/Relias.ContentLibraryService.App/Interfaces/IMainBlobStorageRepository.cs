using Azure.Storage.Blobs.Models;

namespace Relias.ContentLibraryService.App.Interfaces
{
    public interface IMainBlobStorageRepository
    {
        Task<BlobProperties> PersistFileAsync(string container, string path, string fileName, Stream fileStream);

        Task<BlobProperties> GetFilePropertiesAsync(string container, string path, string fileName);


        Task<Stream?> GetFileStreamAsync(string container, string path, string fileName);

        Task<bool> RemoveBlobAsync(string container, string path, string fileName);

        Task<bool> CopyBlobInternalAsync(string sourceContainer, string sourcePath, string destinationPath, string fileName);

        Task<BlockInfo> StageBlockAsync(string container, string path, string fileName, string blockId, Stream fileStream);

        Task<BlobContentInfo> CommitBlockListAsync(string container, string path, string fileName, IEnumerable<string> blockIds);
    }
}