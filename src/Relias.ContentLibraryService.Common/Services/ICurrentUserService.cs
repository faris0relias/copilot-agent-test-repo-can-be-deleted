namespace Relias.ContentLibraryService.Common.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }

        string? OrganizationId { get; }
    }
}