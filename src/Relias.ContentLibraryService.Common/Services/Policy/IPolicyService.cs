using Relias.ContentLibraryService.Common.DTO;

namespace Relias.ContentLibraryService.Common.Services.Policy
{
    /// <summary>
    /// Note Jason Twichell: Dto pattern is onerus, but necessary given the project dependency structure we inherited (domain taking dependency on common).
    /// Prefer to refactor when able to eliminate the need.
    /// </summary>
    public interface IPolicyService
    {
        Task ArchivePolicy(Guid policyId);
        Task UpdatePolicy(PolicyDto policyDto);
        Task CreatePolicy(PolicyDto policyDto);
        Task<PolicyDto?> GetPolicy(Guid policyId);
        Task<PolicyDto?> GetPolicyByContentId(Guid policyId);
        Task<ContentDto> GetContent(Guid contentId);
    }
}
