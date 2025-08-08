using System.ComponentModel.DataAnnotations;

namespace Relias.ContentLibraryService.Domain.Course.ReliasCourse;

public class ReliasCourseProperties
{
    [Key]
    public int ReliasCoursePropertiesId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    public string? Outline { get; set; }

    public string? Disclosure { get; set; }

    public int? CommercialProductDisclaimerId { get; set; }
    public CommercialProductDisclaimer? CommercialProductDisclaimer { get; set; }

    public int? CompletionRequirementId { get; set; }
    public CompletionRequirement? CompletionRequirement { get; set; }

    public int? ContentDisclaimerId { get; set; }
    public ContentDisclaimer? ContentDisclaimer { get; set; }

    public int? CulturalAwarenessStatementId { get; set; }
    public CulturalAwarenessStatement? CulturalAwarenessStatement { get; set; }

    public int? RequestForAccommodationsId { get; set; }
    public RequestForAccommodations? RequestForAccommodations { get; set; }


    public ICollection<LearningObjective> LearningObjectives { get; set; } = [];

    public ICollection<CourseContributor> Contributors { get; set; } = [];
    public ICollection<CourseCareSetting> CareSettings { get; set; } = [];
    public ICollection<CourseTargetAudience> TargetAudiences { get; set; } = [];
    public ICollection<CourseTrainingTopic> TrainingTopics { get; set; } = [];
    public ICollection<CourseDisclosure> Disclosures { get; set; } = [];
}
