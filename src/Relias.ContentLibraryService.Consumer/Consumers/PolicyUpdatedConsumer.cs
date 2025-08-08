using MassTransit;
using Microsoft.Extensions.Options;
using Relias.ComplianceManagement.Contracts.Events;
using Relias.ComplianceManagement.Contracts.Events.ContentLibrary;
using Relias.ContentLibraryService.Common.DTO;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Common.Services.Policy;
using Relias.ContentLibraryService.Consumer.Models;

namespace Relias.ContentLibraryService.Consumer.Consumers
{
    public class PolicyUpdatedConsumer : IConsumer<PolicyUpdatedEvent>
    {
        private readonly ILogger<PolicyUpdatedConsumer> _logger;
        private readonly IPolicyService _policyService;
        private readonly IOptions<BaseSettings> _baseSettings;
        public PolicyUpdatedConsumer(ILogger<PolicyUpdatedConsumer> logger, IPolicyService policyService, IOptions<BaseSettings> baseSettings)
        {
            _logger = logger;
            _policyService = policyService;
            _baseSettings = baseSettings;
        }
        
        public async Task Consume(ConsumeContext<PolicyUpdatedEvent> context)
        {
            _logger.LogInformation("Entering {ConsumerName} {MethodName}.", nameof(PolicyUpdatedConsumer), nameof(Consume));
            _logger.LogInformation("Received PolicyUpdatedEvent message: {Message}", context.Message.Title);

            bool parsed = Guid.TryParse(context.Message.PolicyId, out Guid policyIdGuid);

            if (!parsed)
            {
                _logger.LogError("PolicyId {PolicyId} is not a valid Guid.", context.Message.PolicyId);
                throw new InvalidCastException($"PolicyId {context.Message.PolicyId} is not a valid Guid.");
            }

            try
            {
                PolicyDto? existingPolicy = await _policyService.GetPolicy(policyIdGuid);

                if(existingPolicy == null)
                {
                    _logger.LogError("Policy with PolicyId {PolicyId} not found.", policyIdGuid.ToString());
                    throw new NotFoundException(nameof(PolicyDto), policyIdGuid.ToString());
                }

                // store updated metadata in a new policy object to pass in 
                existingPolicy.PolicyId = policyIdGuid;
                existingPolicy.Title = context.Message.Title;
                existingPolicy.Description = string.IsNullOrEmpty(context.Message.Description) ? null : context.Message.Description;
                existingPolicy.Topic = context.Message.Topic;
                existingPolicy.Subtopic = context.Message.Subtopic;
                existingPolicy.PolicyEventReceivedDate = DateTime.UtcNow;
                existingPolicy.PolicyPublishedDate = context.Message.LastPublishedDate.UtcDateTime;
                existingPolicy.Tags = context.Message.Tags.Select(dtoTag => new PolicyTagDto
                {
                    Value = dtoTag,
                }).ToArray();

                await _policyService.UpdatePolicy(existingPolicy);
                _logger.LogInformation("Processed PolicyUpdatedEvent for PolicyId: {PolicyId}", context.Message.PolicyId);

                await SendContentChangedEvent(context, existingPolicy);
            }
            catch(NotFoundException ex)
            {
                // trap the exception and log it
                // because this application will be concerned with policies that are in published state when its deployed
                // its perfectly likely to receive an update event for a policy that we never knew about                
                _logger.LogError(ex, "Unable to update Policy with PolicyId {PolicyId}, corresponding Policy not found.", policyIdGuid.ToString());                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Something went wrong attempting to update a policy with PolicyId: {policyId}.", policyIdGuid.ToString());
                throw;
            }
        }

        private async Task SendContentChangedEvent(ConsumeContext<PolicyUpdatedEvent> context, PolicyDto policy)
        {
            try
            {
                _logger.LogInformation("Sending ContentChangedEvent for PolicyId: {PolicyId}, ContentId: {ContentId}", policy.PolicyId, policy.ContentId);
                ContentChangedEvent contentChangedEvent = new ContentChangedEvent
                {
                    ContentId = policy.ContentId,
                    OrgId = policy.OwnerOrgId,
                    Title = policy.Title,
                    Topic = policy.Topic,
                    SubTopic = policy.Subtopic,
                    Tags = policy.Tags.Select(tag => tag.Value).ToArray(),
                    ModuleType = ModuleType.Policy,
                    ChangedDate = DateTime.UtcNow
                };

                // there is a shortcut extension to for these ops in MassTransit, however we cannot mock it with Moq, so using this. 
                var sendEndpoint = await context.GetSendEndpoint(new Uri($"topic:{_baseSettings.Value.ContentChangedEventTopicName}"));
                await sendEndpoint.Send(contentChangedEvent);

                _logger.LogInformation("Successfully sent ContentChangedEvent for PolicyId: {PolicyId}, ContentId: {ContentId}", policy.PolicyId, policy.ContentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong attempting to send ContentChangedEvent for PolicyId: {PolicyId}, ContentId: {ContentId}.", policy.PolicyId, policy.ContentId);
            }
        }
    }
}