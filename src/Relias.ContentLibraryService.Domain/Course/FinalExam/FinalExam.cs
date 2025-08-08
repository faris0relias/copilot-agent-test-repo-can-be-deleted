using Microsoft.Azure.CosmosRepository.Attributes;
using Relias.ContentLibraryService.Common;
using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.FinalExam;

[PartitionKeyPath("/courseId")]
public sealed class FinalExam : AuditableEntityCosmos
{
    [Required]
    public Guid CourseId { get; set; }
    public int? MinimumPercentageToPass { get; set; }
    public int? QuestionsDisplayedPerExam { get; set; }
    public Duration? Duration { get; set; }
    public List<FinalExamQuestion>? FinalExamQuestions { get; set; } = [];
}

public sealed class Duration
{
    public int? Hours { get; set; }
    public int? Minutes { get; set; }
}

