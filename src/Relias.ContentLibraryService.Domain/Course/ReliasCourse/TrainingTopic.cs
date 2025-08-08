using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class TrainingTopic
{
    [Key]
    public int TrainingTopicId { get; set; }

    [Required]
    public string Topic { get; set; } = null!;

    public bool Active { get; set; }

    public int QuickbaseRecordId { get; set; }
}
