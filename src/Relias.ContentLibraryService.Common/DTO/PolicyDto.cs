namespace Relias.ContentLibraryService.Common.DTO
{
    public class PolicyDto
    {
        public Guid PolicyId { get; set; }
        public Guid ContentId { get; set; }
        public int OwnerOrgId { get; set; }
        public DateTime PolicyEventReceivedDate { get; set; }
        public DateTime PolicyPublishedDate { get; set; }
        public string Title { get; set; }
        public string Subtopic { get; set; }
        public IEnumerable<PolicyTagDto> Tags { get; set; } = [];
        public string? Description { get; set; }
        public string Topic { get; set; }
    }
}