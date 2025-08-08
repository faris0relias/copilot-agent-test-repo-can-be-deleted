using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.Common.DTO;
using Relias.ContentLibraryService.Common.Repositories;

namespace Relias.ContentLibraryService.Common.Services.Policy
{
    public class PolicyService : IPolicyService
    {
        private readonly ILogger<PolicyService> _logger;
        private readonly IPolicyRepository _policyRepo;
        public PolicyService(ILogger<PolicyService> logger, IPolicyRepository policyRepo) 
        {
            _logger = logger;
            _policyRepo = policyRepo;
        }

        public async Task ArchivePolicy(Guid policyId)
        {
            await _policyRepo.ArchivePolicy(policyId);
        }

        public async Task CreatePolicy(PolicyDto policyDto)
        {
            await _policyRepo.CreatePolicy(policyDto);
        }

        public Task<ContentDto> GetContent(Guid contentId)
        {
            return _policyRepo.GetContent(contentId);
        }

        public Task<PolicyDto?> GetPolicy(Guid policyId)
        {
            return _policyRepo.GetPolicy(policyId);
        }

        public Task<PolicyDto?> GetPolicyByContentId(Guid policyId)
        {
            return _policyRepo.GetPolicyByContentId(policyId);
        }

        public async Task UpdatePolicy(PolicyDto policyDto)
        {
            await _policyRepo.UpdatePolicy(policyDto);
        }
    }
}
