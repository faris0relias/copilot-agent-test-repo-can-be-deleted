using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Common.Constants;
using Relias.ContentLibraryService.Infra.Helpers;

namespace Relias.ContentLibraryService.Infra.Repositories
{
    public class MainBlobStorageRepository(
        IAzureClientFactory<BlobServiceClient> azureClientFactory,
        ILogger<MainBlobStorageRepository> logger)
        : IMainBlobStorageRepository
    {
        private readonly BlobServiceClient _blobServiceClient = azureClientFactory.CreateClient(AzureSdkClientConstants.MainBlobStorage);

        /// <summary>
        /// Saves a file to a container and path with the specified name
        /// </summary>
        /// <param name="container"></param>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        public async Task<BlobProperties> PersistFileAsync(string container, string path, string fileName, Stream fileStream)
        {
           
            var client = BlobStorageHelpers.GetBlobClient(_blobServiceClient, container, path, fileName);

            fileStream.Position = 0;
            await client.UploadAsync(fileStream, true);

            return await client.GetPropertiesAsync();
        }

        /// <summary>
        /// Gets the blob properties of a specific blob
        /// </summary>
        /// <param name="container"></param>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<BlobProperties> GetFilePropertiesAsync(string container, string path, string fileName)
        {           
            var client = BlobStorageHelpers.GetBlobClient(_blobServiceClient, container, path, fileName);
            return await client.GetPropertiesAsync();
        }

        /// <summary>
        /// Gets the stream of a blob for transfer / download
        /// </summary>
        /// <param name="container"></param>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<Stream?> GetFileStreamAsync(string container, string path, string fileName)
        {
            var blobClient = BlobStorageHelpers.GetBlobClient(_blobServiceClient, container, path, fileName);
            return await BlobStorageHelpers.GetFileStreamAsync(blobClient, logger);
        }

        /// <summary>
        /// Deletes the specified blob
        /// </summary>
        /// <param name="container"></param>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<bool> RemoveBlobAsync(string container, string path, string fileName)
        {
            var client = BlobStorageHelpers.GetBlobClient(_blobServiceClient, container, path, fileName);
            return await client.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Deletes the specified blob
        /// </summary>
        /// <param name="sourceContainer"></param>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<bool> CopyBlobInternalAsync(string sourceContainer, string sourcePath, string destinationPath, string fileName)
        {
            try
            {
                var blobClient = BlobStorageHelpers.GetBlobClient(_blobServiceClient, sourceContainer, sourcePath, fileName);

                await blobClient.UploadAsync(destinationPath, true);
                
                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Copy of the BLOB {fileName} from {sourcePath} to {destinationPath} failed.", fileName, sourcePath, destinationPath);
                return false;
            }
        }

        /// <summary>
        /// Stages a block of data for a blob upload using the specified block ID
        /// </summary>
        /// <param name="container">The container name</param>
        /// <param name="path">The blob path</param>
        /// <param name="fileName">The file name</param>
        /// <param name="blockId">The unique block identifier</param>
        /// <param name="fileStream">The stream containing the block data</param>
        /// <returns>Information about the staged block</returns>
        public async Task<BlockInfo> StageBlockAsync(string container, string path, string fileName, string blockId, Stream fileStream)
        {
            var client = BlobStorageHelpers.GetBlockBlobClient(_blobServiceClient, container, path, fileName);
            return await client.StageBlockAsync(blockId, fileStream);
        }

        /// <summary>
        /// Commits a list of staged blocks to create or update a blob
        /// </summary>
        /// <param name="container">The container name</param>
        /// <param name="path">The blob path</param>
        /// <param name="fileName">The file name</param>
        /// <param name="blockIds">The collection of block IDs to commit in order</param>
        /// <returns>Information about the committed blob content</returns>
        public async Task<BlobContentInfo> CommitBlockListAsync(string container, string path, string fileName, IEnumerable<string> blockIds)
        {
            var client = BlobStorageHelpers.GetBlockBlobClient(_blobServiceClient, container, path, fileName);
            return await client.CommitBlockListAsync(blockIds);
        }
    }
}

