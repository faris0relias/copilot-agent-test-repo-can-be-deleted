using Relias.ContentLibraryService.Common;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Certificate
{
    public class Certificate : AuditableEntity
    {
        [Required]
        public Guid CertificateId { get; set; }

        [Required]
        public string Type { get; set; } = null!;

        [Required]
        public byte Status { get; set; }

        public virtual Status.Status CertificateStatus { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public string Board { get; set; } = null!;

        [Required]
        public int CreditHours { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}