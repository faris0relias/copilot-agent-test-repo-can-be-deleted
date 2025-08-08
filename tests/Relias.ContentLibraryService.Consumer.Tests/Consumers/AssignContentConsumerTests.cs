using MassTransit;
using MassTransit.Testing;
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
using Relias.ContentLibraryService.Domain.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Relias.ContentLibraryService.Consumer.Tests.Consumers
{
    public class AssignContentConsumerTests : IAsyncLifetime
    {
        private Mock<ILogger<AssignContentConsumer>> _mockLogger;
        private Mock<IPolicyService> _mockPolicyService;
        private Mock<IOptions<BaseSettings>> _mockBaseSettings;
        private ServiceProvider _provider;       

        public async Task InitializeAsync()
        {
            _mockPolicyService = new Mock<IPolicyService>();
            _mockLogger = new Mock<ILogger<AssignContentConsumer>>();
            _mockBaseSettings = new Mock<IOptions<BaseSettings>>();            
            _provider = new ServiceCollection()
                .AddSingleton(_mockLogger.Object)
                .AddSingleton(_mockPolicyService.Object)
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<AssignContentConsumer>();
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
        public async Task AssignContentConsumer_ShouldSendPolicyContentAssigned_IfPolicyContentType()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            var contentId = Guid.NewGuid();
            var acEvent = new AssignContentEvent
            {
                ContentId = contentId,
                UserId = Guid.NewGuid(),
                LegacyUserId = 1,
                AccessDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(1),
            };

            _mockPolicyService.Setup(_ => _.GetContent(It.IsAny<Guid>())).ReturnsAsync(new ContentDto
            {
                ContentId = contentId,
                ContentTypeDescription = ContentType.Policy
            });

            Guid policyId = Guid.NewGuid();
            _mockPolicyService.Setup(_ => _.GetPolicyByContentId(It.IsAny<Guid>())).ReturnsAsync(new PolicyDto
            {
                ContentId = contentId,
                PolicyId = policyId,
                OwnerOrgId = 1,
                Description = "Description",
                Subtopic = "Subtopic",
                Title = "Title",
                Topic = "Topic",
                PolicyPublishedDate = DateTime.UtcNow,
                PolicyEventReceivedDate = DateTime.UtcNow,
                Tags = [],
            });

            // Act
            await harness.Bus.Publish(acEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<AssignContentEvent>());

            Assert.True(await harness.Sent.Any<PolicyContentAssignedEvent>(pcEvent => 
            pcEvent.Context.Message.PolicyId == policyId &&
            pcEvent.Context.Message.OrgId == 1 &&
            pcEvent.Context.Message.UserId == 1));
        }
        
        [Fact]
        public async Task AssignContentConsumer_ShouldSendPolicyContentAssigned_IfPolicyContentType_WithValidData()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            var dueDate = DateTime.UtcNow.AddDays(1);
            var legacyUserId = 1;
            var ownerOrgId = 2;
            
            var policyId = Guid.NewGuid();
            var contentId = Guid.NewGuid();
            
            var acEvent = new AssignContentEvent
            {
                UserId = Guid.NewGuid(),
                DueDate = dueDate,
                ContentId = Guid.NewGuid(),
                LegacyUserId = legacyUserId,
                AccessDate = DateTime.UtcNow
            };

            _mockPolicyService.Setup(_ => _.GetContent(It.IsAny<Guid>())).ReturnsAsync(new ContentDto
            {
                ContentId = contentId,
                ContentTypeDescription = ContentType.Policy
            });
            
            _mockPolicyService.Setup(_ => _.GetPolicyByContentId(It.IsAny<Guid>())).ReturnsAsync(new PolicyDto
            {
                ContentId = contentId,
                PolicyId = policyId,
                OwnerOrgId = ownerOrgId,
                Description = "Description",
                Subtopic = "Subtopic",
                Title = "Title",
                Topic = "Topic",
                PolicyPublishedDate = DateTime.UtcNow,
                PolicyEventReceivedDate = DateTime.UtcNow,
                Tags = [],
            });

            // Act
            await harness.Bus.Publish(acEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<AssignContentEvent>());

            Assert.True(await harness.Sent.Any<PolicyContentAssignedEvent>(pcEvent => 
                pcEvent.Context.Message.PolicyId == policyId &&
                pcEvent.Context.Message.OrgId == ownerOrgId &&
                pcEvent.Context.Message.UserId == legacyUserId &&
                pcEvent.Context.Message.DueDate == dueDate));
        }

        [Fact]
        public async Task AssignContentConsumer_ShouldThrowException_IfUnsupportedContentType()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            var contentId = Guid.NewGuid();
            var acEvent = new AssignContentEvent
            {
                ContentId = contentId,
                UserId = Guid.NewGuid(),
                LegacyUserId = 1,
                AccessDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(1),
            };

            _mockPolicyService.Setup(_ => _.GetContent(It.IsAny<Guid>())).ReturnsAsync(new ContentDto
            {
                ContentId = contentId,
                ContentTypeDescription = "Unsupported Content Type"
            });

            Guid policyId = Guid.NewGuid();
            _mockPolicyService.Setup(_ => _.GetPolicyByContentId(It.IsAny<Guid>())).ReturnsAsync(new PolicyDto
            {
                ContentId = contentId,
                PolicyId = policyId,
                OwnerOrgId = 1,
                Description = "Description",
                Subtopic = "Subtopic",
                Title = "Title",
                Topic = "Topic",
                PolicyPublishedDate = DateTime.UtcNow,
                PolicyEventReceivedDate = DateTime.UtcNow,
                Tags = [],
            });

            // Act
            await harness.Bus.Publish(acEvent);

            Assert.True(await harness.Consumed.Any<AssignContentEvent>(acEvent => acEvent.Exception != null && acEvent.Exception.GetType() == typeof(NotSupportedException)));
        }

        [Fact]
        public async Task AssignContentConsumer_ShouldThrowNotFoundException_IfContentNotFound()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            var contentId = Guid.NewGuid();
            var acEvent = new AssignContentEvent
            {
                ContentId = contentId,
                UserId = Guid.NewGuid(),
                LegacyUserId = 1,
                AccessDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(1),
            };

            _mockPolicyService.Setup(_ => _.GetContent(It.IsAny<Guid>())).ThrowsAsync(new NotFoundException());

            // Act
            await harness.Bus.Publish(acEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<AssignContentEvent>(acEvent => acEvent.Exception != null && acEvent.Exception.GetType() == typeof(NotFoundException)));
        }

        [Fact]
        public async Task AssignContentConsumer_ShouldThrowNotFoundException_IfPolicyNotFound()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            var contentId = Guid.NewGuid();
            var acEvent = new AssignContentEvent
            {
                ContentId = contentId,
                UserId = Guid.NewGuid(),
                LegacyUserId = 1,
                AccessDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(1),
            };

            _mockPolicyService.Setup(_ => _.GetContent(It.IsAny<Guid>())).ReturnsAsync(new ContentDto
            {
                ContentId = contentId,
                ContentTypeDescription = ContentType.Policy
            });

            _mockPolicyService.Setup(_ => _.GetPolicyByContentId(It.IsAny<Guid>())).ThrowsAsync(new NotFoundException());

            // Act
            await harness.Bus.Publish(acEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<AssignContentEvent>(acEvent => acEvent.Exception != null && acEvent.Exception.GetType() == typeof(NotFoundException)));
        }
    }
}
