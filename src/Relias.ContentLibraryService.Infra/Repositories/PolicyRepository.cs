using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.Common.DTO;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Common.Repositories;
using Relias.ContentLibraryService.Domain.Content;
using Relias.ContentLibraryService.Domain.Policy;
using Relias.ContentLibraryService.Domain.PolicyTag;
using Relias.ContentLibraryService.Infra.Persistence;

namespace Relias.ContentLibraryService.Infra.Repositories
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly ILogger<PolicyRepository> _logger;
        private readonly ApplicationDbContext _context;
        public PolicyRepository(ILogger<PolicyRepository> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ArchivePolicy(Guid policyId)
        {
            Policy? policy = _context.Policy.FirstOrDefault(pol => pol.PolicyId == policyId);
            
            if (policy == null) 
            {               
                _logger.LogDebug("Unable to archive Policy with PolicyId {PolicyId}, corresponding Policy not found.", policyId.ToString());
                throw new NotFoundException(nameof(PolicyDto), policyId.ToString());
            }

            if (!policy.IsArchived)
            {
                policy.IsArchived = true;

                _context.Update(policy);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CreatePolicy(PolicyDto policyDto)
        {
            try
            {
                await _context.Database.BeginTransactionAsync();
                var contentType = await _context.ContentType.FirstOrDefaultAsync(x => x.ContentTypeDescription == "Policy");
                if (contentType == null)
                {
                    _logger.LogDebug("Unable to create new Policy with PolicyId {PolicyId}, content type 'Policy' not found.", policyDto.PolicyId);
                    throw new NotFoundException("ContentType Policy not found.");
                }

                var content = new Content
                {
                    ContentTypeId = contentType.ContentTypeId,
                    ContentId = policyDto.ContentId,
                };

                await _context.AddAsync(content);

                // policy
                var policy = new Policy
                {
                    PolicyId = policyDto.PolicyId,
                    Title = policyDto.Title,
                    ContentId = policyDto.ContentId,
                    OrgId = policyDto.OwnerOrgId,
                    Description = policyDto.Description,
                    Topic = policyDto.Topic,
                    Subtopic = policyDto.Subtopic,
                    IsArchived = false,
                    PolicyEventReceivedDate = DateTime.UtcNow,
                    PolicyPublishedDate = policyDto.PolicyPublishedDate,                   
                };

                await _context.Policy.AddAsync(policy);

                foreach (PolicyTagDto tagDto in policyDto.Tags)
                {
                    Tag? tag = await _context.Tag.FirstOrDefaultAsync(domainTag => domainTag.Value == tagDto.Value);
                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Value = tagDto.Value,
                        };
                    }

                    policy.Tags.Add(tag);
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong attempting to create Policy with PolicyId {PolicyId}.", policyDto.PolicyId);
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ContentDto> GetContent(Guid contentId)
        {
            Content? content = await _context.Content.Include(c => c.ContentType).FirstOrDefaultAsync(c => c.ContentId == contentId);
            if(content == null)
            {
                throw new NotFoundException(nameof(Content), contentId);
            }

            return new ContentDto
            {
                ContentId = content.ContentId,
                ContentTypeDescription = content.ContentType.ContentTypeDescription,
            };
        }

        public async Task<PolicyDto?> GetPolicy(Guid policyId)
        {
            Policy? policy = await _context.Policy.Include(pol => pol.Tags).FirstOrDefaultAsync(policy => policy.PolicyId == policyId);

            if(policy == null)
            {
                return null;
            }

            PolicyDto? policyDto = new PolicyDto
            {
                PolicyId = policyId,
                ContentId = policy.ContentId,
                Title = policy.Title,
                Description = policy.Description,
                Topic = policy.Topic,
                Subtopic = policy.Subtopic,
                OwnerOrgId = policy.OrgId,
                PolicyPublishedDate = policy.PolicyPublishedDate,
                PolicyEventReceivedDate = policy.PolicyEventReceivedDate,
                Tags = policy.Tags.Select(domainTag => new PolicyTagDto
                {
                    TagId = domainTag.TagId,
                    Value = domainTag.Value,
                }).ToArray(),
            }; 

            return policyDto; 
        }

        public async Task<PolicyDto?> GetPolicyByContentId(Guid contentId)
        {
            Policy? policy = await _context.Policy.Include(pol => pol.Tags).FirstOrDefaultAsync(policy => policy.ContentId == contentId);

            if (policy == null)
            {
                return null;
            }
            
            PolicyDto policyDto = new PolicyDto
            {
                PolicyId = policy.PolicyId,
                ContentId = policy.ContentId,
                Title = policy.Title,
                Description = policy.Description,
                Topic = policy.Topic,
                Subtopic = policy.Subtopic,
                OwnerOrgId = policy.OrgId,
                PolicyPublishedDate = policy.PolicyPublishedDate,
                PolicyEventReceivedDate = policy.PolicyEventReceivedDate,
                Tags = policy.Tags.Select(domainTag => new PolicyTagDto
                {
                    TagId = domainTag.TagId,
                    Value = domainTag.Value,
                }).ToArray(),
            };            

            return policyDto;
        }

        public async Task UpdatePolicy(PolicyDto policyDto)
        {
            try
            {
                await _context.Database.BeginTransactionAsync();

                Policy? existingPolicy = _context.Policy.Include(p => p.Tags).FirstOrDefault(pol => pol.PolicyId == policyDto.PolicyId);

                if (existingPolicy != null)
                {
                    // Update the existing Policy object with PolicyDto values
                    existingPolicy.Title = policyDto.Title;
                    existingPolicy.Description = policyDto.Description;
                    existingPolicy.Topic = policyDto.Topic;
                    existingPolicy.Subtopic = policyDto.Subtopic;
                    existingPolicy.PolicyEventReceivedDate = policyDto.PolicyEventReceivedDate;
                    existingPolicy.PolicyPublishedDate = policyDto.PolicyPublishedDate;

                    existingPolicy.Tags.Clear();
                    foreach (PolicyTagDto dtoTag in policyDto.Tags)
                    {
                        Tag? domainTag = _context.Tag.FirstOrDefault(tag => tag.Value == dtoTag.Value);
                        if (domainTag == null)
                        {
                            domainTag = new Tag
                            {
                                Value = dtoTag.Value,
                            };

                            await _context.AddAsync(domainTag);
                        }

                        existingPolicy.Tags.Add(domainTag);
                    }
                }
                else
                {
                    _logger.LogDebug("Unable to update Policy with PolicyId {PolicyId}, corresponding Policy not found.", policyDto.PolicyId.ToString());
                    throw new NotFoundException(nameof(PolicyDto), policyDto.PolicyId.ToString());
                }

                _context.Update(existingPolicy);

                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong attempting to update Policy with PolicyId {PolicyId}.", policyDto.PolicyId);
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
