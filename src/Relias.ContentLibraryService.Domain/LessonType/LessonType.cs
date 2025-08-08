using Relias.ContentLibraryService.Common;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.LessonType
{
    public class LessonType : AuditableEntity
    {
        [Required]
        public Guid LessonTypeId { get; set; }

        [Required]
        public string? LessonTypeName { get; set; }

        public virtual ICollection<Lesson.Lesson> Lessons { get; set; } = [];
    }
}