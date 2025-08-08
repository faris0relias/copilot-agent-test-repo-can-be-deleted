namespace Relias.ContentLibraryService.Domain.Content
{
    public class Content
    {
        public Guid ContentId { get; set; }
        public int ContentTypeId { get; set; }

        public ContentType.ContentType ContentType { get; set; } = null!;
    }
}