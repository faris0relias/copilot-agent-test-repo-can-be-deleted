using System.Security.Claims;
using Relias.ContentLibraryService.Api.Utilities;
using Relias.ContentLibraryService.Api.Authorization.Exceptions;

namespace Relias.ContentLibraryService.Api.Tests.Utilities
{
    public class ClaimsPrincipalExtensionsTests
    {
        [Fact]
        public void OrganizationId_WhenValid_ReturnsOrganizationId()
        {
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(UserTokenKeys.OrganizationId, "123")
            }));

            var orgId = userClaims.OrganizationId();

            Assert.Equal(123, orgId);
        }

        [Fact]
        public void OrganizationId_WhenMissing_ThrowsClaimsAccessorException()
        {
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity());

            var exception = Assert.Throws<ClaimsAccessorException>(() => userClaims.OrganizationId());

            Assert.Equal("Unable to determine token key from current HTTP Context", exception.Message);
        }

        [Fact]
        public void OrganizationIds_WhenValid_ReturnsOrganizationIds()
        {
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(UserTokenKeys.OrganizationIds, "10"),
                new Claim(UserTokenKeys.OrganizationIds, "20"),
                new Claim(UserTokenKeys.OrganizationIds, "30")
            }));

            var orgIds = userClaims.OrganizationIds();

            Assert.Contains(10, orgIds);
            Assert.Contains(20, orgIds);
            Assert.Contains(30, orgIds);
        }

        [Fact]
        public void OrganizationIds_WhenNoClaims_ReturnsEmptyList()
        {
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity());

            var orgIds = userClaims.OrganizationIds();

            Assert.Empty(orgIds);
        }
    }
}
