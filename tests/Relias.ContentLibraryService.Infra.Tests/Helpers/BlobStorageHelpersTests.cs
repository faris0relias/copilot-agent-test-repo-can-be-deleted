using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Relias.ContentLibraryService.Infra.Helpers;
using System.Text;


namespace Relias.ContentLibraryService.Infra.Tests.Helpers
{
    public class BlobStorageHelpersTests
    {
        private readonly Mock<ILogger> _mockLogger = new();
        private readonly Mock<BlobServiceClient> _mockBlobServiceClient = new();
        private readonly Mock<BlobContainerClient> _mockBlobContainerClient = new();
        private readonly Mock<BlobClient> _mockBlobClient = new();

        [Fact]
        public void GetBlobClient_ReturnsCorrectBlobClient()
        {
            // Arrange
            var container = "test-container";
            var path = "test/path/";
            var blobName = "test-blob.txt";
            var expectedPathWithBlobName = "test/path/test-blob.txt";

            _mockBlobServiceClient.Setup(s => s.GetBlobContainerClient(container)).Returns(_mockBlobContainerClient.Object);
            _mockBlobContainerClient.Setup(c => c.GetBlobClient(expectedPathWithBlobName)).Returns(_mockBlobClient.Object);

            // Act
            var result = BlobStorageHelpers.GetBlobClient(_mockBlobServiceClient.Object, container, path, blobName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_mockBlobClient.Object, result);
            _mockBlobServiceClient.Verify(s => s.GetBlobContainerClient(container), Times.Once);
            _mockBlobContainerClient.Verify(c => c.GetBlobClient(expectedPathWithBlobName), Times.Once);
        }

        [Fact]
        public async Task GetFileStreamAsync_ReturnsStream_WhenDownloadIsSuccessful()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var content = "Test content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var blobDownloadResult = BlobsModelFactory.BlobDownloadResult(content: BinaryData.FromStream(stream));
            var mockResponse = new Mock<Response<BlobDownloadResult>>();
            mockResponse.SetupGet(r => r.Value).Returns(blobDownloadResult);

            _mockBlobClient.Setup(c => c.DownloadContentAsync(cancellationToken)).ReturnsAsync(mockResponse.Object);

            // Act
            var result = await BlobStorageHelpers.GetFileStreamAsync(_mockBlobClient.Object, _mockLogger.Object, cancellationToken);

            // Assert
            Assert.NotNull(result);
            using var reader = new StreamReader(result);
            var resultContent = await reader.ReadToEndAsync();
            Assert.Equal(content, resultContent);
            _mockBlobClient.Verify(c => c.DownloadContentAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetFileStreamAsync_ReturnsNull_WhenDownloadFailsWith404()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            _mockBlobClient.Setup(c => c.DownloadContentAsync(cancellationToken)).ThrowsAsync(new RequestFailedException(404, "Not Found"));

            // Act
            var result = await BlobStorageHelpers.GetFileStreamAsync(_mockBlobClient.Object, _mockLogger.Object, cancellationToken);

            // Assert
            Assert.Null(result);
            _mockBlobClient.Verify(c => c.DownloadContentAsync(cancellationToken), Times.Exactly(5));
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Download Content failed. Retry attempt: 1")),
                    It.IsAny<RequestFailedException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Fact]
        public async Task GetFileStreamAsync_RetriesAndSucceeds_WhenDownloadFailsWithOtherException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var content = "Test content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var blobDownloadResult = BlobsModelFactory.BlobDownloadResult(content: BinaryData.FromStream(stream));
            var mockResponse = new Mock<Response<BlobDownloadResult>>();
            mockResponse.SetupGet(r => r.Value).Returns(blobDownloadResult);

            _mockBlobClient.SetupSequence(c => c.DownloadContentAsync(cancellationToken))
                .ThrowsAsync(new RequestFailedException("Service unavailable"))
                .ReturnsAsync(mockResponse.Object);

            // Act
            var result = await BlobStorageHelpers.GetFileStreamAsync(_mockBlobClient.Object, _mockLogger.Object, cancellationToken);

            // Assert
            Assert.NotNull(result);
            _mockBlobClient.Verify(c => c.DownloadContentAsync(cancellationToken), Times.Exactly(2));
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Download Content failed. Retry attempt: 1")),
                    It.IsAny<RequestFailedException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Fact]
        public async Task GetFileStreamAsync_ReturnsNull_AfterMaxRetries()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            _mockBlobClient.Setup(c => c.DownloadContentAsync(cancellationToken)).ThrowsAsync(new RequestFailedException("Service unavailable"));

            // Act
            var result = await BlobStorageHelpers.GetFileStreamAsync(_mockBlobClient.Object, _mockLogger.Object, cancellationToken);

            // Assert
            Assert.Null(result);
            _mockBlobClient.Verify(c => c.DownloadContentAsync(cancellationToken), Times.Exactly(5)); // MaxRetryCount is 5
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Download Content failed. Retry attempt: ")),
                    It.IsAny<RequestFailedException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Exactly(5));
        }
    }
}
