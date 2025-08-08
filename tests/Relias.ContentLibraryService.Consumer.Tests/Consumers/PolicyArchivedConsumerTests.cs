using MassTransit;
using MassTransit.Testing;
using Microsoft.Azure.Amqp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Relias.ComplianceManagement.Contracts.Events;
using Relias.ComplianceManagement.Contracts.Events.ContentScheduler;
using Relias.ContentLibraryService.Common.DTO;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Common.Services.Policy;
using Relias.ContentLibraryService.Consumer.Consumers;
using Relias.ContentLibraryService.Consumer.Models;
using Relias.ContentScheduler.Consumer.Consumers;
using Xunit;
using Assert = Xunit.Assert;

namespace Relias.ContentLibraryService.Consumer.Tests.Consumers
{    
    public class PolicyArchivedConsumerTests: IAsyncLifetime
    {
        private Mock<ILogger<PolicyArchivedConsumer>> _mockLogger;
        private Mock<IPolicyService> _mockPolicyService;
        private ServiceProvider _provider;

        public async Task InitializeAsync()
        {
            _mockPolicyService = new Mock<IPolicyService>();
            _mockLogger = new Mock<ILogger<PolicyArchivedConsumer>>();
            _provider = new ServiceCollection()
                .AddSingleton(_mockLogger.Object)
                .AddSingleton(_mockPolicyService.Object)
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<PolicyArchivedConsumer>();
                })
                .BuildServiceProvider(true);

            var harness = _provider.GetRequiredService<ITestHarness>();
            await harness.Start();
        }

        public async Task DisposeAsync()
        {
            if (_provider != null)
            {
                await _provider.DisposeAsync();
            }
        }

        [Fact]
        public async Task PolicyArchivedConsumer_ShouldThrowInvalidCastException_IfGuidNotParsable()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();
            var paEvent = new PolicyArchivedEvent
            {
                PolicyId = "Not a guid",
                ArchivedDate = DateTimeOffset.UtcNow,                
            };

            // Act
            await harness.Bus.Publish(paEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<PolicyArchivedEvent>(paEvent => paEvent.Exception != null && paEvent.Exception.GetType() == typeof(InvalidCastException)));
        }

        [Fact]
        public async Task PolicyArchivedConsumer_ShouldTrapNotFoundException_IfPolicyNotFound()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();
            var paEvent = new PolicyArchivedEvent
            {
                PolicyId = Guid.NewGuid().ToString(),
                ArchivedDate = DateTimeOffset.UtcNow,
            };

            _mockPolicyService.Setup(_ => _.ArchivePolicy(It.IsAny<Guid>())).ThrowsAsync(new NotFoundException());

            // Act
            await harness.Bus.Publish(paEvent);

            // Assert
            Assert.False(await harness.Consumed.Any<PolicyArchivedEvent>(paEvent => paEvent.Exception != null && paEvent.Exception.GetType() == typeof(NotFoundException)));
        }

        [Fact]
        public async Task PolicyArchivedConsumer_ShouldCallService_IfValidMessage()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();
            var paEvent = new PolicyArchivedEvent
            {
                PolicyId = Guid.NewGuid().ToString(),
                ArchivedDate = DateTimeOffset.UtcNow,
            };

            // Act
            await harness.Bus.Publish(paEvent);

            // Assert 
            Assert.True(await harness.Consumed.Any<PolicyArchivedEvent>());
            _mockPolicyService.Verify(_ => _.ArchivePolicy(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task PolicyArchivedConsumer_ShouldSendContentArchivedEvent_IfValidMessage()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            Guid policyId = Guid.NewGuid();
            Guid contentId = Guid.NewGuid();
            var paEvent = new PolicyArchivedEvent
            {
                PolicyId = policyId.ToString(),
                ArchivedDate = DateTimeOffset.UtcNow,
            };

            _mockPolicyService.Setup(_ => _.GetPolicy(It.IsAny<Guid>()))
                .ReturnsAsync(new PolicyDto
                {
                    ContentId = contentId,
                    PolicyId = policyId,                   
                });

            // Act
            await harness.Bus.Publish(paEvent);

            // Assert 
            Assert.True(await harness.Consumed.Any<PolicyArchivedEvent>());
            _mockPolicyService.Verify(_ => _.ArchivePolicy(It.IsAny<Guid>()), Times.Once);

            Assert.True(await harness.Sent.Any<ContentArchivedEvent>(caEvent => 
                caEvent.Context.Message.ContentId == contentId));
        }

        [Fact]
        public async Task PolicyArchivedConsumer_ShouldNotSendContentArchivedEvent_IfPolicyNotFound()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();
                     
            var paEvent = new PolicyArchivedEvent
            {
                PolicyId = Guid.NewGuid().ToString(),
                ArchivedDate = DateTimeOffset.UtcNow,
            };

            _mockPolicyService.Setup(_ => _.GetPolicy(It.IsAny<Guid>())).ThrowsAsync(new NotFoundException());

            // Act
            await harness.Bus.Publish(paEvent);

            // Assert 
            Assert.True(await harness.Consumed.Any<PolicyArchivedEvent>());
            _mockPolicyService.Verify(_ => _.ArchivePolicy(It.IsAny<Guid>()), Times.Once);

            Assert.False(await harness.Sent.Any<ContentArchivedEvent>());
        }
    }
}
