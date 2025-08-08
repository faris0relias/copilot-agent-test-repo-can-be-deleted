namespace Relias.ContentLibraryService.App.Features.Course.Dtos.ReliasCourse;

public class ReliasCoursePropertiesDto
{
    public string? Outline { get; set; }
    public string? Disclosure { get; set; }
    public string? CommercialProductDisclaimer { get; set; }
    public string? CompletionRequirement { get; set; }
    public string? ContentDisclaimer { get; set; }
    public string? CulturalAwarenessStatement { get; set; }
    public string? RequestForAccommodations { get; set; }

    public List<ContributorDto> Contributors { get; set; } = [];
    public List<string> LearningObjectives { get; set; } = [];
    public List<string> CareSettings { get; set; } = [];
    public List<string> TargetAudiences { get; set; } = [];
    public List<string> TrainingTopics { get; set; } = [];
    public List<string> Disclosures { get; set; } = [];
}

public class ContributorDto
{
    public int? ContributorIdNum { get; set; }

    public string? NameAndCredentials { get; set; }

    public string? ShortBio { get; set; }

    public string? DisclosureStatement { get; set; }
}
