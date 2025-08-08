using MassTransit;
using Microsoft.Extensions.Options;
using Relias.ComplianceManagement.Contracts.Events;
using Relias.ContentLibraryService.Common.DTO;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Common.Services.Policy;
using Relias.ContentLibraryService.Consumer.Models;
using Relias.ContentScheduler.Consumer.Consumers;

namespace Relias.ContentLibraryService.Consumer.Consumers
{
    public class PolicyArchivedConsumer : IConsumer<PolicyArchivedEvent>
    {
        private readonly ILogger<PolicyArchivedConsumer> _logger;
        private readonly IPolicyService _policyService;
        private readonly IOptions<BaseSettings> _baseSettings;
        public PolicyArchivedConsumer(ILogger<PolicyArchivedConsumer> logger, IPolicyService policyService, IOptions<BaseSettings> baseSettings)
        {
            _logger = logger;
            _policyService = policyService;
            _baseSettings = baseSettings;
        }

        public async Task Consume(ConsumeContext<PolicyArchivedEvent> context)
        {
            _logger.LogInformation("Entering {ConsumerName} {MethodName}.", nameof(PolicyArchivedConsumer), nameof(Consume));

            bool parsed = Guid.TryParse(context.Message.PolicyId, out Guid policyIdGuid);

            if (!parsed)
            {
                _logger.LogError("PolicyId {PolicyId} is not a valid Guid.", context.Message.PolicyId);
                throw new InvalidCastException($"PolicyId {context.Message.PolicyId} is not a valid Guid.");
            }

            try
            {
                await _policyService.ArchivePolicy(policyIdGuid);
                await SendContentArchived(context, policyIdGuid);
            }
            catch (NotFoundException ex)
            {
                // trap the exception and log it
                // because this application will be concerned with policies that are in published state when its deployed
                // its perfectly likely to receive an archive event for a policy that we never knew about                
                _logger.LogError(ex, "Unable to archive Policy with PolicyId {PolicyId}, corresponding Policy not found.", policyIdGuid.ToString());                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong while attempting to archive a policy with PolicyId: {policyId}.", policyIdGuid.ToString());
                throw;
            }

            _logger.LogInformation("Leaving {ConsumerName} {MethodName}.", nameof(PolicyArchivedConsumer), nameof(Consume));
        }

        private async Task SendContentArchived(ConsumeContext<PolicyArchivedEvent> context, Guid policyId)
        {
            _logger.LogInformation("Entering {DelegateMethodName}.", nameof(SendContentArchived));

            PolicyDto? policy = await _policyService.GetPolicy(policyId);

            if (policy == null)
            {
                _logger.LogError("Policy with PolicyID {PolicyId} not found.", policyId.ToString());
                throw new NotFoundException(nameof(PolicyDto), policyId.ToString());
            }

            _logger.LogInformation("Policy found, sending {ContentArchived} for ContentId: {ContentId}.", nameof(ContentArchivedEvent), policy.ContentId.ToString());

            ContentArchivedEvent contentArchivedEvent = new ContentArchivedEvent
            {
                ContentId = policy.ContentId
            };

            var sendEndpoint = await context.GetSendEndpoint(new Uri($"topic:{_baseSettings.Value.ContentArchivedEventTopicName}"));
            await sendEndpoint.Send(contentArchivedEvent);

            _logger.LogInformation("{ContentArchivedEvent} sent for PolicyId: {PolicyId}, ContentId: {ContentId}. Leaving {DelegateMethodName}.", nameof(ContentArchivedEvent), policy.PolicyId, policy.ContentId, nameof(SendContentArchived));
        }
    }
}

namespace Relias.ContentScheduler.Consumer.Consumers
{
    public class ContentArchivedEvent
    {
        public Guid ContentId { get; set; }
    }
}
