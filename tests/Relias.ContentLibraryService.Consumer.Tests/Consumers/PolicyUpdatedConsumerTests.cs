using MassTransit;
using MassTransit.Testing;
using MassTransit.Transports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Relias.ComplianceManagement.Contracts.Events;
using Relias.ComplianceManagement.Contracts.Events.ContentLibrary;
using Relias.ComplianceManagement.Contracts.Events.ContentScheduler;
using Relias.ContentLibraryService.Common.DTO;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Common.Services.Policy;
using Relias.ContentLibraryService.Consumer.Consumers;
using Relias.ContentLibraryService.Consumer.Models;
using Relias.ContentLibraryService.Domain.Content;
using Relias.ContentLibraryService.Infra.Persistence;
using Xunit;
using Assert = Xunit.Assert;

namespace Relias.ContentLibraryService.Consumer.Tests.Consumers
{
    public class PolicyUpdatedConsumerTests : IAsyncLifetime
    {
        private Mock<ILogger<PolicyUpdatedConsumer>> _mockLogger;
        private Mock<IPolicyService> _mockPolicyService;
        private Mock<IOptions<BaseSettings>> _mockBaseSettings;
        private ServiceProvider _provider;

        public async Task InitializeAsync()
        {
            _mockPolicyService = new Mock<IPolicyService>();
            _mockLogger = new Mock<ILogger<PolicyUpdatedConsumer>>();
            _mockBaseSettings = new Mock<IOptions<BaseSettings>>();
            _provider = new ServiceCollection()
                .AddSingleton(_mockLogger.Object)
                .AddSingleton(_mockPolicyService.Object)
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<PolicyUpdatedConsumer>();
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
        public async Task PolicyUpdatedConsumer_ShouldThrowInvalidCastException_IfGuidNotParsable()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();
            var puEvent = new PolicyUpdatedEvent
            {
                PolicyId = "Not a guid",
            };

            // Act
            await harness.Bus.Publish(puEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<PolicyUpdatedEvent>(puEvent => puEvent.Exception != null && puEvent.Exception.GetType() == typeof(InvalidCastException)));
        }

        [Fact]
        public async Task PolicyUpdatedConsumer_ShouldCallPolicyServiceUpdate_IfValidMessage()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            string policyId = Guid.NewGuid().ToString();
            DateTimeOffset lastUpdated = DateTimeOffset.UtcNow;
            var policyUpdatedEvent = new PolicyUpdatedEvent()
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

            Guid contentId = Guid.NewGuid();
            var expectedPolicyDto = new PolicyDto()
            {
                PolicyId = new Guid(policyId),
                ContentId = contentId,
                Title = policyUpdatedEvent.Title,
                Description = policyUpdatedEvent.Description,
                Topic = policyUpdatedEvent.Topic,
                Subtopic = policyUpdatedEvent.Subtopic,
                OwnerOrgId = 123,
                PolicyPublishedDate = lastUpdated.UtcDateTime,
                Tags = policyUpdatedEvent.Tags.Select(tag => new PolicyTagDto { Value = tag }).ToList()
            };

            _mockPolicyService.Setup(_ => _.GetPolicy(It.IsAny<Guid>())).ReturnsAsync(new PolicyDto()
            {
                PolicyId = new Guid(policyId),
                ContentId = contentId
            });

            // Act
            await harness.Bus.Publish(policyUpdatedEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<PolicyUpdatedEvent>());

            // Assert
            _mockPolicyService.Verify(_ => _.UpdatePolicy(It.Is<PolicyDto>(dto =>
                dto.PolicyId.ToString() == policyId &&
                dto.ContentId == contentId &&
                dto.Title == expectedPolicyDto.Title &&
                dto.Description == expectedPolicyDto.Description &&
                dto.Topic == expectedPolicyDto.Topic &&
                dto.Subtopic == expectedPolicyDto.Subtopic &&
                dto.PolicyPublishedDate == expectedPolicyDto.PolicyPublishedDate &&
                dto.Tags.Select(tag => tag.Value).SequenceEqual(expectedPolicyDto.Tags.Select(tag => tag.Value))
            )), Times.Once);
        }

        [Fact]
        public async Task PolicyUpdateConsumer_ShouldNotThrowNotFoundException_WhenPolicyNotFound()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            string policyId = Guid.NewGuid().ToString();
            DateTimeOffset lastUpdated = DateTimeOffset.UtcNow;
            var policyUpdatedEvent = new PolicyUpdatedEvent()
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
            Assert.False(await harness.Consumed.Any<PolicyUpdatedEvent>(puEvent => puEvent.Exception != null && puEvent.Exception.GetType() == typeof(NotFoundException)));
        }

        [Fact]
        public async Task PolicyUpdatedConsumer_ShouldSendContentChangedEvent()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            string policyId = Guid.NewGuid().ToString();
            DateTimeOffset lastUpdated = DateTimeOffset.UtcNow;
            var policyUpdatedEvent = new PolicyUpdatedEvent()
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

            Guid contentId = Guid.NewGuid();
            var expectedPolicyDto = new PolicyDto()
            {
                PolicyId = new Guid(policyId),
                ContentId = contentId,
                Title = policyUpdatedEvent.Title,
                Description = policyUpdatedEvent.Description,
                Topic = policyUpdatedEvent.Topic,
                Subtopic = policyUpdatedEvent.Subtopic,
                OwnerOrgId = 123,
                PolicyPublishedDate = lastUpdated.UtcDateTime,
                Tags = policyUpdatedEvent.Tags.Select(tag => new PolicyTagDto { Value = tag }).ToList()
            };

            _mockPolicyService.Setup(_ => _.GetPolicy(It.IsAny<Guid>())).ReturnsAsync(new PolicyDto()
            {
                PolicyId = new Guid(policyId),
                ContentId = contentId,
                OwnerOrgId = 123
            });

            // Act
            await harness.Bus.Publish(policyUpdatedEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<PolicyUpdatedEvent>());

            Assert.True(await harness.Sent.Any<ContentChangedEvent>(ccEvent =>
                ccEvent.Context.Message.ContentId == contentId &&
                ccEvent.Context.Message.OrgId == 123 &&
                ccEvent.Context.Message.Title == expectedPolicyDto.Title &&
                ccEvent.Context.Message.Tags.SequenceEqual(expectedPolicyDto.Tags.Select(t => t.Value)) &&
                ccEvent.Context.Message.Topic == expectedPolicyDto.Topic &&
                ccEvent.Context.Message.SubTopic == expectedPolicyDto.Subtopic));
        }
    }
}