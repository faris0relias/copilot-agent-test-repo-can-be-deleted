using MassTransit;
using Microsoft.Extensions.Options;
using Relias.ComplianceManagement.Contracts.Events;
using Relias.ComplianceManagement.Contracts.Events.ContentScheduler;
using Relias.ContentLibraryService.Common.DTO;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Common.Services.Policy;
using Relias.ContentLibraryService.Consumer.Models;
using ContentType = Relias.ContentLibraryService.Consumer.Models.ContentType;

namespace Relias.ContentLibraryService.Consumer.Consumers
{
    public class AssignContentConsumer : IConsumer<AssignContentEvent>
    {
        private readonly ILogger<AssignContentConsumer> _logger;
        private readonly IPolicyService _policyService;
        private readonly IOptions<BaseSettings> _baseSettings;
        public AssignContentConsumer(ILogger<AssignContentConsumer> logger, IPolicyService policyService, IOptions<BaseSettings> baseSettings)
        {
            _logger = logger;
            _policyService = policyService;
            _baseSettings = baseSettings;
        }

        public async Task Consume(ConsumeContext<AssignContentEvent> context)
        {
            _logger.LogInformation("Entering {ConsumerName} {MethodName}.", nameof(AssignContentConsumer), nameof(Consume));

            try
            {
                ContentDto content = await _policyService.GetContent(context.Message.ContentId);

                switch (content.ContentTypeDescription)
                {
                    case ContentType.Policy:
                        await SendPolicyContentAssigned(context, context.Message.ContentId);
                        return;
                    default:
                        _logger.LogError("ContentId {ContentId} is not a supported ContentType.", context.Message.ContentId); 
                        throw new NotSupportedException($"ContentId {context.Message.ContentId} is not a supported ContentType.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong while processing {EventName}.", nameof(AssignContentEvent));
                throw;
            }
        }

        private async Task SendPolicyContentAssigned(ConsumeContext<AssignContentEvent> context, Guid contentId)
        {
            _logger.LogInformation("Entering {DelegateMethodName}.", nameof(SendPolicyContentAssigned));

            PolicyDto? policy = await _policyService.GetPolicyByContentId(contentId);

            if (policy == null)
            {
                _logger.LogError("Policy with ContentId {ContentId} not found.", contentId.ToString());
                throw new NotFoundException(nameof(PolicyDto), contentId.ToString());
            }

            _logger.LogInformation("Policy content found, sending {PolicyContentAssigned} for PolicyId: {PolicyId}.", nameof(PolicyContentAssignedEvent), policy.PolicyId.ToString());
            
            PolicyContentAssignedEvent polAssignedEvent = new PolicyContentAssignedEvent
            {
                OrgId = policy.OwnerOrgId,
                UserId = context.Message.LegacyUserId ?? throw new ArgumentNullException("LegacyUserId cannot be null"),
                PolicyId = policy.PolicyId,
                DueDate = context.Message.DueDate
            };

            var sendEndpoint = await context.GetSendEndpoint(new Uri($"topic:{_baseSettings.Value.PolicyContentAssignedEventTopicName}"));
            await sendEndpoint.Send(polAssignedEvent);

            _logger.LogInformation("PolicyContentAssignedEvent sent for PolicyId: {PolicyId}, OrgId: {OrgId}, UserId: {UserId}. Leaving {DelegateMethodName}.", polAssignedEvent.PolicyId, polAssignedEvent.OrgId, polAssignedEvent.UserId, nameof(SendPolicyContentAssigned));
        }
    }
}
