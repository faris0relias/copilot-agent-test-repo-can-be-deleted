using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Common.Constants;
using Relias.ContentLibraryService.Function.Function;
using Relias.ContentLibraryService.Function.Models;
using System.Text.Json;

namespace Relias.ContentLibraryService.Function.Tests
{
    public class BlobEventTests
    {
        private readonly Mock<ILogger<BlobEvent>> _loggerMock;
        private readonly Mock<IMainBlobStorageRepository> _repositoryMock;
        private readonly BlobEvent _blobEvent;
        private readonly string _blobUriString = "https://teststorage.blob.core.windows.net/main/8/aaaa0000-bb11-2222-33cc-444444dddddd/aaaa0000-bb11-2222-33cc-444444dddddd/fileName.ext";
        private readonly string _malformedBlobUriString = "https://teststorage.blob.core.windows.net/main/8/courseIdGuid/lessonIdGuid/"; // Missing fileName
        private readonly string _invalidContainerBlobUriString = "https://teststorage.blob.core.windows.net/othercontainer/8/aaaa0000-bb11-2222-33cc-444444dddddd/aaaa0000-bb11-2222-33cc-444444dddddd/fileName.ext";

        public BlobEventTests()
        {
            _loggerMock = new Mock<ILogger<BlobEvent>>();
            _repositoryMock = new Mock<IMainBlobStorageRepository>();
            _blobEvent = new BlobEvent(_loggerMock.Object, _repositoryMock.Object);
        }

        [Fact]
        public async Task BlobResultEvent_WhenFilePathOrFileNameIsNullOrEmpty_ShouldThrowArgumentException()
        {
            var eventData = JsonSerializer.SerializeToDocument(new Dictionary<string, string>
            {
                { "scanResultType", "Malicious" },
                { "blobUri", _malformedBlobUriString }
            });

            var eventGridEvent = new EventGridEvent(
                "Microsoft.Storage/storageAccounts/teststorage",
                "Microsoft.Security.MalwareScanningResult",
                "1.0",
                BinaryData.FromString(eventData.RootElement.ToString()));

            await Assert.ThrowsAsync<ArgumentException>(() => _blobEvent.BlobResultEvent(eventGridEvent, CancellationToken.None));
        }

        //[Fact] Restore this test with RPLAT-5668
        //public async Task BlobResultEvent_WhenFailedVerdict_ShouldDeleteBlob()
        //{
        //    var eventData = JsonSerializer.SerializeToDocument(new Dictionary<string, string>
        //    {
        //        { "scanResultType", "Failed" },
        //        { "blobUri", _blobUriString }
        //    });

        //    var eventGridEvent = new EventGridEvent(
        //        "Microsoft.Storage/storageAccounts/teststorage",
        //        "Microsoft.Security.MalwareScanningResult",
        //        "1.0",
        //        BinaryData.FromString(eventData.RootElement.ToString()));

        //    _repositoryMock
        //        .Setup(r => r.RemoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        //    await _blobEvent.BlobResultEvent(eventGridEvent, CancellationToken.None);

        //    _repositoryMock.Verify(
        //        r => r.RemoveBlobAsync(
        //            AzureSdkClientConstants.MainContainer,
        //            "8/aaaa0000-bb11-2222-33cc-444444dddddd/aaaa0000-bb11-2222-33cc-444444dddddd",
        //            "fileName.ext"),
        //        Times.Once);
        //}

        [Fact]
        public void ParseBlobInfoFromUri_WithValidUri_ReturnsBlobInfo()
        {
            var blobUri = new Uri(_blobUriString);
            var method = typeof(BlobEvent).GetMethod("ParseBlobInfoFromUri", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            var result = method.Invoke(_blobEvent, new object[] { blobUri });
            Assert.NotNull(result);
        }

        [Fact]
        public void ParseBlobInfoFromUri_WithInvalidUri_ThrowsArgumentException()
        {
            var blobUri = new Uri("https://teststorage.blob.core.windows.net/main/8/courseIdGuid/lessonIdGuid/");
            var method = typeof(BlobEvent).GetMethod("ParseBlobInfoFromUri", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            Assert.Throws<System.Reflection.TargetInvocationException>(() => method.Invoke(_blobEvent, new object[] { blobUri }));
        }

        [Fact]
        public void ScanResultEventData_Deserialization_Works()
        {
            var json = "{\"scanResultType\":\"Malicious\",\"blobUri\":\"https://teststorage.blob.core.windows.net/main/8/aaaa0000-bb11-2222-33cc-444444dddddd/aaaa0000-bb11-2222-33cc-444444dddddd/fileName.ext\"}";
            var result = JsonSerializer.Deserialize<ScanResultEventData>(json);
            Assert.Equal("Malicious", result.scanResultType);
            Assert.Contains("fileName.ext", result.blobUri);
        }

        [Fact]
        public async Task BlobResultEvent_WhenEventTypeIsNotAntimalwareScan_ShouldReturnEarly()
        {
            // Arrange
            var eventGridEvent = new EventGridEvent(
                "test-subject",
                "NotAnAntimalwareScanType",
                "1.0",
                JsonSerializer.Serialize(new { }));

            // Act
            await _blobEvent.BlobResultEvent(eventGridEvent, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Event type is not an") && o.ToString()!.Contains("NotAnAntimalwareScanType")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            
            _repositoryMock.Verify(r => r.RemoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task BlobResultEvent_WhenNotInterestedContainer_ShouldReturnEarly()
        {
            // Arrange
            var eventData = JsonSerializer.SerializeToDocument(new Dictionary<string, string> 
            {
                { "scanResultType", "No threats found" },
                { "blobUri", _invalidContainerBlobUriString }
            });
            
            var eventGridEvent = new EventGridEvent(
                "Microsoft.Storage/storageAccounts/teststorage", // Subject
                "Microsoft.Security.MalwareScanningResult", // Event Type
                "1.0",
                BinaryData.FromString(eventData.RootElement.ToString()));

            // Act
            await _blobEvent.BlobResultEvent(eventGridEvent, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Event is not from the interested containers")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            
            _repositoryMock.Verify(r => r.RemoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        //[Fact] Restore this test with RPLAT-5668
        //public async Task BlobResultEvent_WhenMaliciousVerdict_ShouldDeleteBlob()
        //{
        //    // Arrange
        //    var eventData = JsonSerializer.SerializeToDocument(new Dictionary<string, string> 
        //    {
        //        { "scanResultType", "Malicious" }, 
        //        { "blobUri", _blobUriString }
        //    });

        //    var eventGridEvent = new EventGridEvent(
        //        "Microsoft.Storage/storageAccounts/teststorage", // Subject
        //        "Microsoft.Security.MalwareScanningResult", // Event Type
        //        "1.0",
        //        BinaryData.FromString(eventData.RootElement.ToString()));

        //    _repositoryMock
        //        .Setup(r => r.RemoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        //    // Act
        //    await _blobEvent.BlobResultEvent(eventGridEvent, CancellationToken.None);

        //    // Assert
        //    _repositoryMock.Verify(
        //        r => r.RemoveBlobAsync(
        //            AzureSdkClientConstants.MainContainer,
        //            "8/aaaa0000-bb11-2222-33cc-444444dddddd/aaaa0000-bb11-2222-33cc-444444dddddd",
        //            "fileName.ext"),
        //        Times.Once);

        //    _loggerMock.Verify(
        //        x => x.Log(
        //            LogLevel.Information,
        //            It.IsAny<EventId>(),
        //            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("blob fileName.ext is Malicious, deleting it from main container")),
        //            It.IsAny<Exception>(),
        //            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        //        Times.Once);
        //}

        //[Fact] Restore this test with RPLAT-5668
        //public async Task BlobResultEvent_WhenDeleteBlobFails_ShouldLogErrorAndRethrow()
        //{
        //    // Arrange
        //    var eventData = JsonSerializer.SerializeToDocument(new Dictionary<string, string> 
        //    {
        //        {"scanResultType", "Malicious"}, 
        //        { "blobUri", _blobUriString }
        //    });

        //    var eventGridEvent = new EventGridEvent(
        //        "Microsoft.Storage/storageAccounts/teststorage", // Subject
        //        "Microsoft.Security.MalwareScanningResult", // Event Type
        //        "1.0",
        //        BinaryData.FromString(eventData.RootElement.ToString()));

        //    var exception = new Exception("Failed to delete blob");
        //    _repositoryMock
        //        .Setup(r => r.RemoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        //        .ThrowsAsync(exception);

        //    // Act & Assert
        //    await Assert.ThrowsAsync<Exception>(() => _blobEvent.BlobResultEvent(eventGridEvent, CancellationToken.None));

        //    _loggerMock.Verify(
        //        x => x.Log(
        //            LogLevel.Error,
        //            It.IsAny<EventId>(),
        //            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error deleting blob")),
        //            exception,
        //            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        //        Times.Once);
        //}

        [Fact]
        public async Task BlobResultEvent_WhenCleanVerdict_ShouldNotThrowException()
        {
            // Arrange
            var eventData = JsonSerializer.SerializeToDocument(new Dictionary<string, string> 
            {
                { "scanResultType", "No threats found" }, 
                { "blobUri", _blobUriString }
            });
            
            var eventGridEvent = new EventGridEvent(
                "Microsoft.Storage/storageAccounts/teststorage", // Subject
                "Microsoft.Security.MalwareScanningResult", // Event Type
                "1.0",
                BinaryData.FromString(eventData.RootElement.ToString()));

            // Act - this should not throw an exception
            await _blobEvent.BlobResultEvent(eventGridEvent, CancellationToken.None);

            // Assert
            // The current implementation only has a placeholder comment for clean verdicts
            // so there's no specific action to verify other than it doesn't throw an exception
            _repositoryMock.Verify(r => r.RemoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task BlobResultEvent_WhenMissingVerdictOrBlobUri_ShouldThrowArgumentException()
        {
            // Arrange
            var eventData = JsonSerializer.SerializeToDocument(new Dictionary<string, string> 
            {
                // scanResultType is missing
                { "verdict", "value" }
            });
            
            var eventGridEvent = new EventGridEvent(
                "Microsoft.Storage/storageAccounts/teststorage", // Subject
                "Microsoft.Security.MalwareScanningResult", // Event Type
                "1.0",
                BinaryData.FromString(eventData.RootElement.ToString()));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _blobEvent.BlobResultEvent(eventGridEvent, CancellationToken.None));
        }

        [Fact]
        public async Task BlobResultEvent_WhenBlobUriIsMalformed_ShouldThrowUriFormatException()
        {
            var eventData = JsonSerializer.SerializeToDocument(new Dictionary<string, string>
            {
                { "scanResultType", "Malicious" },
                { "blobUri", "not-a-valid-uri" }
            });

            var eventGridEvent = new EventGridEvent(
                "Microsoft.Storage/storageAccounts/teststorage",
                "Microsoft.Security.MalwareScanningResult",
                "1.0",
                BinaryData.FromString(eventData.RootElement.ToString()));

            await Assert.ThrowsAsync<UriFormatException>(() => _blobEvent.BlobResultEvent(eventGridEvent, CancellationToken.None));
        }

        [Fact]
        public async Task BlobResultEvent_WhenScanResultTypeIsUnknown_ShouldThrowArgumentException()
        {
            var eventData = JsonSerializer.SerializeToDocument(new Dictionary<string, string>
            {
                { "scanResultType", "Unknown" },
                { "blobUri", _blobUriString }
            });

            var eventGridEvent = new EventGridEvent(
                "Microsoft.Storage/storageAccounts/teststorage",
                "Microsoft.Security.MalwareScanningResult",
                "1.0",
                BinaryData.FromString(eventData.RootElement.ToString()));

            await Assert.ThrowsAsync<ArgumentException>(() => _blobEvent.BlobResultEvent(eventGridEvent, CancellationToken.None));

            _repositoryMock.Verify(r => r.RemoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Event data contains unexpected 'scanResultType' value:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

    }
}
