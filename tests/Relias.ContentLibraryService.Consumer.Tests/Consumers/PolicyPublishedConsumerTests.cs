using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Relias.ContentLibraryService.Common.DTO;
using Relias.ContentLibraryService.Common.Services.Policy;
using Relias.ContentLibraryService.Consumer.Consumers;
using Relias.ComplianceManagement.Contracts.Events;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using Relias.ComplianceManagement.Contracts.Events.ContentLibrary;
using Relias.ContentLibraryService.Domain.Content;
using Relias.ContentLibraryService.Common.Exceptions;
using Microsoft.Extensions.Options;
using Relias.ContentLibraryService.Consumer.Models;

namespace Relias.ContentLibraryService.Consumer.Tests.Consumers
{
    public class PolicyPublishedConsumerTests : IAsyncLifetime
    {
        private ServiceProvider _provider;
        private Mock<IPolicyService> _mockPolicyService;
        private Mock<IOptions<BaseSettings>> _mockBaseSettings;
        private Mock<ILogger<PolicyPublishedConsumer>> _mockLogger;

        public async Task InitializeAsync()
        {
            _mockPolicyService = new Mock<IPolicyService>();
            _mockLogger = new Mock<ILogger<PolicyPublishedConsumer>>();
            _mockBaseSettings = new Mock<IOptions<BaseSettings>>();
            _provider = new ServiceCollection()
                .AddSingleton(_mockLogger.Object)
                .AddSingleton(_mockPolicyService.Object)
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<PolicyPublishedConsumer>();
                })
                .AddSingleton(_mockBaseSettings)
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
        public async Task PolicyPublishedConsumer_ShouldThrowInvalidCastException_IfGuidNotParsable()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();
            var ppEvent = new PolicyPublishedEvent
            {
                PolicyId = "Not a guid",
            };

            // Act
            await harness.Bus.Publish(ppEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<PolicyPublishedEvent>(ppEvent => ppEvent.Exception != null && ppEvent.Exception.GetType() == typeof(InvalidCastException)));
        }

        [Fact]
        public async Task PolicyPublishedConsumer_ShouldCallCreate_IfNewPolicy()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            var policyPublishedEvent = new PolicyPublishedEvent
            {
                PolicyId = Guid.NewGuid().ToString(),
                Title = "Test Title",
                Description = "Test Description",
                Topic = "Test Topic",
                Subtopic = "Test Subtopic",
                OwnerOrgId = 123,
                LastPublishedDate = DateTimeOffset.UtcNow,
                Tags = ["1", "2", "3"]
            };

            _mockPolicyService.Setup(svc => svc.GetPolicy(It.IsAny<Guid>())).ReturnsAsync((PolicyDto?)null);

            // Act
            await harness.Bus.Publish(policyPublishedEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<PolicyPublishedEvent>());

            _mockPolicyService.Verify(_ => _.CreatePolicy(It.Is<PolicyDto>(dto =>
                dto.PolicyId == Guid.Parse(policyPublishedEvent.PolicyId) &&
                dto.Title == policyPublishedEvent.Title &&
                dto.Description == policyPublishedEvent.Description &&
                dto.Topic == policyPublishedEvent.Topic &&
                dto.Subtopic == policyPublishedEvent.Subtopic &&
                dto.PolicyPublishedDate == policyPublishedEvent.LastPublishedDate.UtcDateTime &&
                dto.OwnerOrgId == policyPublishedEvent.OwnerOrgId &&
                dto.Tags.Select(tag => tag.Value).SequenceEqual(policyPublishedEvent.Tags)
            )), Times.Once);
        }

        [Fact]
        public async Task PolicyPublishedConsumer_ShouldCallUpdate_IfExistingPolicy()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            var policyId = Guid.NewGuid();
            var policyPublishedEvent = new PolicyPublishedEvent
            {
                PolicyId = policyId.ToString(),
                Title = "Updated Title",
                Description = "Updated Description",
                Topic = "Updated Topic",
                Subtopic = "Updated Subtopic",
                OwnerOrgId = 123,
                LastPublishedDate = DateTimeOffset.UtcNow,
                Tags = ["1", "2", "3"]
            };

            var existingPolicy = new PolicyDto
            {
                PolicyId = policyId,
                Title = "Old Title",
                Description = "Old Description",
                Topic = "Old Topic",
                Subtopic = "Old Subtopic",
                OwnerOrgId = 123,
                PolicyPublishedDate = DateTime.UtcNow.AddDays(-1),
                Tags = [new PolicyTagDto { Value = "OldTag" }]
            };

            _mockPolicyService.Setup(svc => svc.GetPolicy(policyId)).ReturnsAsync(existingPolicy);

            // Act
            await harness.Bus.Publish(policyPublishedEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<PolicyPublishedEvent>());

            _mockPolicyService.Verify(_ => _.UpdatePolicy(It.Is<PolicyDto>(dto =>
                dto.PolicyId == policyId &&
                dto.Title == policyPublishedEvent.Title &&
                dto.Description == policyPublishedEvent.Description &&
                dto.Topic == policyPublishedEvent.Topic &&
                dto.Subtopic == policyPublishedEvent.Subtopic &&
                dto.PolicyPublishedDate == policyPublishedEvent.LastPublishedDate.UtcDateTime &&
                dto.OwnerOrgId == policyPublishedEvent.OwnerOrgId &&
                dto.Tags.Select(tag => tag.Value).SequenceEqual(policyPublishedEvent.Tags)
            )), Times.Once);
        }

        [Fact]
        public async Task PolicyPublishedConsumer_ShouldSendContentChangedEvent()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            var policyId = Guid.NewGuid();
            var policyPublishedEvent = new PolicyPublishedEvent
            {
                PolicyId = policyId.ToString(),
                Title = "Updated Title",
                Description = "Updated Description",
                Topic = "Updated Topic",
                Subtopic = "Updated Subtopic",
                OwnerOrgId = 123,
                LastPublishedDate = DateTimeOffset.UtcNow,
                Tags = ["1", "2", "3"]
            };

            var contentId = Guid.NewGuid();
            var existingPolicy = new PolicyDto
            {
                PolicyId = policyId,
                ContentId = contentId,
                Title = "Old Title",
                Description = "Old Description",
                Topic = "Old Topic",
                Subtopic = "Old Subtopic",
                OwnerOrgId = 123,
                PolicyPublishedDate = DateTime.UtcNow.AddDays(-1),
                Tags = [new PolicyTagDto { Value = "OldTag" }]
            };

            _mockPolicyService.Setup(svc => svc.GetPolicy(policyId)).ReturnsAsync(existingPolicy);

            // Act
            await harness.Bus.Publish(policyPublishedEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<PolicyPublishedEvent>());

            Assert.True(await harness.Sent.Any<ContentChangedEvent>(ccEvent =>
                ccEvent.Context.Message.ContentId == contentId &&
                ccEvent.Context.Message.OrgId == 123 &&
                ccEvent.Context.Message.Title == existingPolicy.Title &&
                ccEvent.Context.Message.Tags.SequenceEqual(existingPolicy.Tags.Select(t => t.Value)) &&
                ccEvent.Context.Message.Topic == existingPolicy.Topic &&
                ccEvent.Context.Message.SubTopic == existingPolicy.Subtopic));
        }

        [Fact]
        public async Task PolicyPublishedConsumer_ShouldThrowNotFoundException_WhenPolicyNotFound()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            string policyId = Guid.NewGuid().ToString();
            DateTimeOffset lastUpdated = DateTimeOffset.UtcNow;
            var policyUpdatedEvent = new PolicyPublishedEvent()
            {
                PolicyId = policyId,
                Title = "Test Title",
                Description = "Test Description",
                Topic = "Test Topic",
                Subtopic = "Test Subtopic",
                OwnerOrgId = 123,
                LastPublishedDate = lastUpdated,
                Tags = ["1", "2", "3"]
            };

            _mockPolicyService
               .Setup(s => s.GetPolicy(It.IsAny<Guid>()))
               .ThrowsAsync(new NotFoundException("Policy not found"));

            // Act
            await harness.Bus.Publish(policyUpdatedEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<PolicyPublishedEvent>(puEvent => puEvent.Exception != null && puEvent.Exception.GetType() == typeof(NotFoundException)));
        }
    }
}
