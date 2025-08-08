using MassTransit;
using MassTransit.Testing;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Relias.ContentLibraryService.Consumer.Tests.Consumers
{
    public class PolicyAttestationCompletedConsumerTests : IAsyncLifetime
    {
        private Mock<ILogger<PolicyAttestationCompletedConsumer>> _mockLogger;
        private Mock<IPolicyService> _mockPolicyService;
        private Mock<IOptions<BaseSettings>> _mockBaseSettings;
        private ServiceProvider _provider;

        public async Task InitializeAsync()
        {
            _mockPolicyService = new Mock<IPolicyService>();
            _mockLogger = new Mock<ILogger<PolicyAttestationCompletedConsumer>>();
            _mockBaseSettings = new Mock<IOptions<BaseSettings>>();
            _provider = new ServiceCollection()
                .AddSingleton(_mockLogger.Object)
                .AddSingleton(_mockPolicyService.Object)
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<PolicyAttestationCompletedConsumer>();
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
        public async Task AssignContentConsumer_ShouldSendContentCompletedEvent_IfPolicyContentFound()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            Guid policyId = Guid.NewGuid();
            DateTime completedDate = DateTime.UtcNow;
            PolicyAttestationCompletedEvent pacEvent = new PolicyAttestationCompletedEvent
            {
                PolicyId = policyId,
                OrgId = 1,
                UserId = 1,
                DateAttestationCompleted = completedDate
            };

            Guid contentId = Guid.NewGuid();
            _mockPolicyService.Setup(_ => _.GetPolicy(It.IsAny<Guid>())).ReturnsAsync(new PolicyDto
            {
                PolicyId = policyId,
                ContentId = contentId,
            });

            // Act
            await harness.Bus.Publish(pacEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<PolicyAttestationCompletedEvent>());

            Assert.True(await harness.Sent.Any<ContentCompletedEvent>(pcEvent => 
                pcEvent.Context.Message.ContentId == contentId &&
                pcEvent.Context.Message.LegacyUserId == 1 &&
                pcEvent.Context.Message.CompletedDate == completedDate
            ));
        }

        [Fact]
        public async Task AssignContentConsumer_ShouldThrowNotFoundException_IfPolicyNotFound()
        {
            // Arrange
            var harness = _provider.GetRequiredService<ITestHarness>();

            Guid policyId = Guid.NewGuid();
            DateTime completedDate = DateTime.UtcNow;
            PolicyAttestationCompletedEvent pacEvent = new PolicyAttestationCompletedEvent
            {
                PolicyId = policyId,
                OrgId = 1,
                UserId = 1,
                DateAttestationCompleted = completedDate
            };

            Guid contentId = Guid.NewGuid();
            _mockPolicyService.Setup(_ => _.GetPolicy(It.IsAny<Guid>())).ThrowsAsync(new NotFoundException());

            // Act
            await harness.Bus.Publish(pacEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<PolicyAttestationCompletedEvent>(pcEvent => pcEvent.Equals != null && pcEvent.Exception.GetType() == typeof(NotFoundException)));
        }
    }
}
