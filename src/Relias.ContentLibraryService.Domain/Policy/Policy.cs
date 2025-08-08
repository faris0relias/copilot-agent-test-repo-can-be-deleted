using Relias.ContentLibraryService.Domain.PolicyTag;

namespace Relias.ContentLibraryService.Domain.Policy
{
    public class Policy
    {
        public Guid ContentId { get; set; }
        public Guid PolicyId { get; set; }
        public int OrgId { get; set; }
        public DateTime PolicyEventReceivedDate { get; set; }
        public DateTime PolicyPublishedDate { get; set; }
        public string Title { get; set; }
        public string Subtopic { get; set; }
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public string? Description { get; set; }
        public string Topic { get; set; }
        public bool IsArchived { get; set; }
    }
}