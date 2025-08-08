using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Moq;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Infra.Repositories;

namespace Relias.ContentLibraryService.Infra.Tests.Repositories
{
    public class MainBlobStorageRepositoryTests
    {
        private readonly Mock<IAzureClientFactory<BlobServiceClient>> _azureClientFactoryMock;
        private readonly Mock<ILogger<MainBlobStorageRepository>> _loggerMock;
        private readonly Mock<BlobClient> _blobClientMock;

        public MainBlobStorageRepositoryTests()
        {
            _azureClientFactoryMock = new Mock<IAzureClientFactory<BlobServiceClient>>();
            _loggerMock = new Mock<ILogger<MainBlobStorageRepository>>();
            Mock<BlobServiceClient> blobServiceClientMock = new();
            Mock<BlobContainerClient> blobContainerClientMock = new();
            _blobClientMock = new Mock<BlobClient>();

            blobContainerClientMock
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClientMock.Object);

            blobServiceClientMock
                .Setup(c => c.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(blobContainerClientMock.Object);

            _blobClientMock
                .Setup(c => c.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>(r => r.Value == BlobsModelFactory.BlobContentInfo(new ETag("etag"), DateTimeOffset.UtcNow, null, null, 1)));

            _blobClientMock
                .Setup(c => c.UploadAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())) // For CopyBlobInternalAsync
                .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>(r => r.Value == BlobsModelFactory.BlobContentInfo(new ETag("etag"), DateTimeOffset.UtcNow, null, null, 1)));

            _blobClientMock
                .Setup(c => c.SetMetadataAsync(
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(null as Response<BlobInfo>);

            _blobClientMock
                .Setup(c => c.DeleteIfExistsAsync(
                    It.IsAny<DeleteSnapshotsOption>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<bool>>(r => r.Value == true));

            var mockBlobProperties = Mock.Of<BlobProperties>();
            _blobClientMock
                .Setup(b => b.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<BlobProperties>>(r => r.Value == mockBlobProperties));

            var testData = new byte[] { 1, 2, 3, 4, 5 };
            var memoryStream = new MemoryStream(testData);
            
            var binaryData = BinaryData.FromStream(memoryStream);
            var blobDownloadDetails = BlobsModelFactory.BlobDownloadDetails(
                lastModified: DateTimeOffset.UtcNow,
                blobType: BlobType.Block);
            var blobDownloadResult = BlobsModelFactory.BlobDownloadResult(
                content: binaryData,
                details: blobDownloadDetails);
            
            // Create and setup the mock response
            var mockResponse = new Mock<Response<BlobDownloadResult>>();
            mockResponse.Setup(r => r.Value).Returns(blobDownloadResult);

            _blobClientMock
                .Setup(c => c.DownloadContentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _blobClientMock
                .Setup(c => c.DownloadContentAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _azureClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(blobServiceClientMock.Object);
        }

        [Fact]
        public async Task PersistFileAsync_WhenMalwareFound_UploadsFileAndStartsBackgroundScan()
        {
            // Arrange
            var container = "main";
            var path = "org-1/V1";
            var fileName = "test.pdf";
            var fileStream = new MemoryStream([1, 2, 3]);

            var repo = new MainBlobStorageRepository(
                _azureClientFactoryMock.Object,
                _loggerMock.Object);

            // Act 
            var result = await repo.PersistFileAsync(container, path, fileName, fileStream);

            // Assert
            Assert.NotNull(result); 
            _blobClientMock.Verify(b => b.UploadAsync(It.IsAny<Stream>(), true, CancellationToken.None), Times.Once);
            _blobClientMock.Verify(b => b.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PersistFileAsync_WhenNoMalwareFound_UploadsAndReturnsProperties()
        {
            // Arrange
            var container = "main";
            var path = "org-1/P-1/V1";
            var fileName = "test.pdf";
            var fileStream = new MemoryStream([1, 2, 3]);

            var repo = new MainBlobStorageRepository(
                _azureClientFactoryMock.Object,
                _loggerMock.Object);

            // Act
            await repo.PersistFileAsync(container, path, fileName, fileStream);

            // Assert
            _blobClientMock.Verify(b => b.UploadAsync(It.IsAny<Stream>(), true, CancellationToken.None), Times.Once);
            _blobClientMock.Verify(b => b.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetFilePropertiesAsync_ReturnsProperties()
        {
            // Arrange
            var container = "main";
            var path = "org-1/V1";
            var fileName = "test.pdf";
            var expectedProperties = await _blobClientMock.Object.GetPropertiesAsync();

            var repo = new MainBlobStorageRepository(
                _azureClientFactoryMock.Object,
                _loggerMock.Object);

            // Act
            var result = await repo.GetFilePropertiesAsync(container, path, fileName);

            // Assert
            Assert.Equal(expectedProperties.Value, result);
            _blobClientMock.Verify(b => b.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Exactly(2)); // Once in arrange, once in act
        }

        [Fact]
        public async Task GetFileStreamAsync_ReturnsStream()
        {
            // Arrange
            var container = "main";
            var path = "org-1/V1";
            var fileName = "test.pdf";

            var repo = new MainBlobStorageRepository(
                _azureClientFactoryMock.Object,
                _loggerMock.Object);

            // Act
            var resultStream = await repo.GetFileStreamAsync(container, path, fileName);

            // Assert
            Assert.NotNull(resultStream);
            Assert.True(resultStream.Length > 0); // Check if stream has content
            _blobClientMock.Verify(c => c.DownloadContentAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CopyBlobInternalAsync_Success_ReturnsTrue()
        {
            // Arrange
            var sourceContainer = "container";
            var sourcePath = "source/path";
            var destinationBlobName = "dest/path/copied.pdf"; 
            var fileName = "source.pdf";

            var repo = new MainBlobStorageRepository(
                _azureClientFactoryMock.Object,
                _loggerMock.Object);

            // Act
            var result = await repo.CopyBlobInternalAsync(sourceContainer, sourcePath, destinationBlobName, fileName);

            // Assert
            Assert.True(result);
            _blobClientMock.Verify(c => c.UploadAsync(destinationBlobName, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CopyBlobInternalAsync_Failure_WhenUploadAsyncThrows_ReturnsFalseAndLogsError()
        {
            // Arrange
            var sourceContainer = "container";
            var sourcePath = "source/path";
            var destinationBlobName = "dest/path/copied.pdf";
            var fileName = "source.pdf";

            _blobClientMock.Setup(c => c.UploadAsync(destinationBlobName, true, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new Exception("Simulated upload failure"));

            var repo = new MainBlobStorageRepository(
                _azureClientFactoryMock.Object,
                _loggerMock.Object);

            // Act
            var result = await repo.CopyBlobInternalAsync(sourceContainer, sourcePath, destinationBlobName, fileName);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Copy of the BLOB {fileName} from {sourcePath} to {destinationBlobName} failed.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }
    }
}
