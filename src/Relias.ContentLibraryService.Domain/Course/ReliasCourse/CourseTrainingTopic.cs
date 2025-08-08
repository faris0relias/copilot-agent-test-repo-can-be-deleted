using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class CourseTrainingTopic
{
    [Key]
    public int CourseTrainingTopicsId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public int TrainingTopicId { get; set; }

    public TrainingTopic TrainingTopic { get; set; } = null!;
}
