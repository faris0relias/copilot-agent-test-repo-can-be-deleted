using Microsoft.Azure.CosmosRepository;

namespace Relias.ContentLibraryService.Common
{
    public abstract class AuditableEntityCosmos : FullItem
    {
        public required DateTime Created { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
