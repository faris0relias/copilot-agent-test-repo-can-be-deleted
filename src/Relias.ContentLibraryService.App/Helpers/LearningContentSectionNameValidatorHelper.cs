using FluentValidation.Results;
using Microsoft.Azure.Cosmos.Linq;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using System.Reflection;

namespace Relias.ContentLibraryService.App.Helpers;
public static class SectionNameValidatorHelper
{
    // Validates Section Names are unique per course and that English name has a value
    public static List<ValidationFailure> GetSectionNameErrors(IEnumerable<LearningContentSectionDto> sections)
    {
        List<ValidationFailure> failures = [];

        foreach (LearningContentSectionDto section in sections)
        {
            if (section.Name == null)
            {
                failures.Add(new ValidationFailure(
                    propertyName: $"Sections.Name",
                    errorMessage: $"Name is required for section update - Section Id:{section.SectionId}"
                ));
                continue;
            }

            PropertyInfo[] nameProps = section.Name.GetType().GetProperties();
            foreach (PropertyInfo prop in nameProps)
            {
                string language = prop.Name;
                string? value = prop.GetValue(section.Name)?.ToString();

                if (language.Equals(nameof(LocalizedStringDto.En)) && string.IsNullOrWhiteSpace(value))
                {
                    failures.Add(new ValidationFailure(
                        propertyName: $"Sections.Name.{language}",
                        errorMessage: $"Name.En cannot be empty - Section Id: {section.SectionId}"
                        ));
                }
            }
        }
        return failures;
    }
}
