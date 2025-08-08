using MassTransit;
using Microsoft.Extensions.Options;
using Relias.ComplianceManagement.Contracts.Events;
using Relias.ComplianceManagement.Contracts.Events.ContentLibrary;
using Relias.ComplianceManagement.Contracts.Events.ContentScheduler;
using Relias.ContentLibraryService.Common.DTO;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Common.Services.Policy;
using Relias.ContentLibraryService.Consumer.Models;

namespace Relias.ContentLibraryService.Consumer.Consumers
{
    public class PolicyAttestationCompletedConsumer : IConsumer<PolicyAttestationCompletedEvent>
    {
        private readonly ILogger<PolicyAttestationCompletedConsumer> _logger;
        private readonly IPolicyService _policyService;
        private readonly IOptions<BaseSettings> _baseSettings;
        public PolicyAttestationCompletedConsumer(ILogger<PolicyAttestationCompletedConsumer> logger, IPolicyService policyService, IOptions<BaseSettings> baseSettings)
        {
            _logger = logger;
            _policyService = policyService;
            _baseSettings = baseSettings;
        }

        public async Task Consume(ConsumeContext<PolicyAttestationCompletedEvent> context)
        {
            _logger.LogInformation("Entering {ConsumerName} {MethodName}.", nameof(PolicyAttestationCompletedConsumer), nameof(Consume));

            try
            {
                PolicyDto? policy = await _policyService.GetPolicy(context.Message.PolicyId);

                if (policy == null) 
                {
                    _logger.LogError("Policy with PolicyId {PolicyId} not found.", context.Message.PolicyId);
                    throw new NotFoundException(nameof(PolicyDto), context.Message.PolicyId);
                }

                _logger.LogInformation("Policy content found, sending {ContentCompletedEvent} for PolicyId: {PolicyId}, ContentId: {ContentId}.", nameof(ContentCompletedEvent), policy.PolicyId.ToString(), policy.ContentId.ToString());
                ContentCompletedEvent contentCompletedEvent = new ContentCompletedEvent
                {
                    ContentId = policy.ContentId,
                    LegacyUserId = context.Message.UserId,
                    CompletedDate = context.Message.DateAttestationCompleted,
                    UserId = null,
                };

                var sendEndpoint = await context.GetSendEndpoint(new Uri($"topic:{_baseSettings.Value.ContentCompletedEventTopicName}"));
                await sendEndpoint.Send(contentCompletedEvent);

                _logger.LogInformation("Successfully sent {ContentCompletedEvent} for PolicyId: {PolicyId}, ContentId: {ContentId}.", nameof(ContentCompletedEvent), policy.PolicyId.ToString(), policy.ContentId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong while processing {EventName}.", nameof(PolicyAttestationCompletedEvent));
                throw;
            }
        }
    }
}
