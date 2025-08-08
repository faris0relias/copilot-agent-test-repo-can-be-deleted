
namespace Relias.ContentLibraryService.Common.Services
{
    public class CurrentUserServiceStub : ICurrentUserService
    {
        public string? UserId => new Guid("ac7a4f9a-bc0e-4fa1-b27f-0e15ab1ca15d").ToString();

        public string? OrganizationId => "8";
    }
}