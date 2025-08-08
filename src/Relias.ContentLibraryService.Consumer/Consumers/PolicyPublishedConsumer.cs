using MassTransit;
using Microsoft.Extensions.Options;
using Relias.ComplianceManagement.Contracts.Events;
using Relias.ComplianceManagement.Contracts.Events.ContentLibrary;
using Relias.ContentLibraryService.Common.DTO;
using Relias.ContentLibraryService.Common.Services.Policy;
using Relias.ContentLibraryService.Consumer.Models;

namespace Relias.ContentLibraryService.Consumer.Consumers
{
    public class PolicyPublishedConsumer : IConsumer<PolicyPublishedEvent>
    {
        private readonly ILogger<PolicyPublishedConsumer> _logger;
        private readonly IPolicyService _policyService;
        private readonly IOptions<BaseSettings> _baseSettings;
        public PolicyPublishedConsumer(ILogger<PolicyPublishedConsumer> logger, IPolicyService policyService, IOptions<BaseSettings> baseSettings)
        {
            _logger = logger;
            _policyService = policyService;
            _baseSettings = baseSettings;
        }

        public async Task Consume(ConsumeContext<PolicyPublishedEvent> context)
        {
            _logger.LogInformation("Received PolicyPublishedEvent for PolicyId: {PolicyId}", context.Message.PolicyId);

            bool parsed = Guid.TryParse(context.Message.PolicyId, out Guid policyIdGuid);

            if (!parsed)
            {
                _logger.LogError("PolicyId {PolicyId} is not a valid Guid.", context.Message.PolicyId);
                throw new InvalidCastException($"PolicyId {context.Message.PolicyId} is not a valid Guid.");
            }

            try
            {
                PolicyDto? policy = await _policyService.GetPolicy(policyIdGuid);
                if (policy == null)
                {
                    policy = new PolicyDto
                    {
                        PolicyId = policyIdGuid,
                        ContentId = Guid.NewGuid(),
                        OwnerOrgId = context.Message.OwnerOrgId,
                        Title = context.Message.Title,
                        Description = string.IsNullOrEmpty(context.Message.Description) ? null : context.Message.Description,
                        Topic = context.Message.Topic,
                        Subtopic = context.Message.Subtopic,
                        PolicyEventReceivedDate = DateTime.UtcNow,
                        PolicyPublishedDate = context.Message.LastPublishedDate.UtcDateTime,
                        Tags = context.Message.Tags.Select(t => new PolicyTagDto
                        {
                            Value = t,
                        }).ToArray()
                    };

                    await _policyService.CreatePolicy(policy);
                }
                else
                {
                    policy.Title = context.Message.Title;
                    policy.Description = string.IsNullOrEmpty(context.Message.Description) ? null : context.Message.Description;
                    policy.Topic = context.Message.Topic;
                    policy.Subtopic = context.Message.Subtopic;
                    policy.PolicyEventReceivedDate = DateTime.UtcNow;
                    policy.PolicyPublishedDate = context.Message.LastPublishedDate.UtcDateTime;
                    policy.Tags = context.Message.Tags.Select(t => new PolicyTagDto
                    {
                        Value = t,
                    }).ToArray();

                    await _policyService.UpdatePolicy(policy);
                }

                _logger.LogInformation("Processed PolicyPublishedEvent for PolicyId: {PolicyId}", context.Message.PolicyId);

                await SendContentChangedEvent(context, policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong while attempted to upsert policy with PolicyId: {policyId}.", policyIdGuid.ToString());
                throw;
            }
        }

        private async Task SendContentChangedEvent(ConsumeContext<PolicyPublishedEvent> context, PolicyDto policy)
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