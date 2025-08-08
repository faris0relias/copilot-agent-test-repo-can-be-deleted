using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;

namespace Relias.ContentLibraryService.Infra.Helpers
{
    public static class BlobStorageHelpers
    {
        private const int MaxRetryCount = 5;

        /// <summary>
        /// Gets the stream of a blob for transfer / download
        /// </summary>
        /// <param name="blobClient"></param>
        /// <param name="logger"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Stream?> GetFileStreamAsync(BlobClient blobClient, ILogger logger, CancellationToken cancellationToken = default)
        {
            Stream? result = null;
            var retryCount = 1;

            while (result is null && retryCount <= MaxRetryCount)
            {
                try
                {
                    var response = await blobClient.DownloadContentAsync(cancellationToken);
                    result = response.Value.Content.ToStream();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Download Content failed. Retry attempt: {retryCount}", retryCount);
                    if (ex.Message.Contains("404"))
                    {
                        // abort retries
                        return null;
                    }
                }
                retryCount++;
            }

            return result;
        }

        public static BlobClient GetBlobClient(BlobServiceClient blobServiceClient, string container, string path, string blobName)
        {
            var pathWithBlobName = FileHelper.SanitizePath(path) + blobName;

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(container);
            var blobClient = blobContainerClient.GetBlobClient(pathWithBlobName);

            return blobClient;
        }
        
        public static BlockBlobClient GetBlockBlobClient(BlobServiceClient blobServiceClient, string container, string path, string blobName)
        {
            var pathWithBlobName = FileHelper.SanitizePath(path) + blobName;

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(container);
            var blockBlobClient = blobContainerClient.GetBlockBlobClient(pathWithBlobName);

            return blockBlobClient;
        }
    }
}