namespace Relias.ContentLibraryService.Function.Models
{
    public class BlobMetadata
    {
        public string? ContainerName { get; init; }
        public int OrgId { get; init; }
        public Guid CourseId { get; init; }
        public Guid LessonId { get; init; }
        public string? FilePath { get; init; }
        public string? FileName { get; init; }
    }
}
