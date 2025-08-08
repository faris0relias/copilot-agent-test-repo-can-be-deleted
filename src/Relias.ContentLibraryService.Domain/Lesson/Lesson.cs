using Relias.ContentLibraryService.Common;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Lesson
{
    public class Lesson : AuditableEntity
    {
        [Required]
        public Guid LessonId { get; set; }

        [Required]
        public byte StatusId { get; set; }

        [Required]
        public Guid LessonTypeId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Duration { get; set; }

        [Required]
        public List<Guid> TopicIds { get; set; }
        public virtual LessonType.LessonType LessonType { get; set; } = null!;
        public virtual Status.Status Status { get; set; } = null!;
    }
}