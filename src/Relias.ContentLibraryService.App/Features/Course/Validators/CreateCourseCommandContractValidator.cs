using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using System.Text.RegularExpressions;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class CreateCourseCommandContractValidator : AbstractValidator<CreateCourseCommand.Contract>
{
    private const int ContentCodeMaxLength = 100;
    private const int CourseNameMaxLength = 500;

    public CreateCourseCommandContractValidator()
    {
        // Defines Rules that should only run if Content Code is not empty
        var contentCodeNotEmptyRules = () =>
        {
            RuleFor(v => v.NewCourse.ContentCode)
                .MaximumLength(ContentCodeMaxLength)
                .WithMessage($"Content code can not exceed {ContentCodeMaxLength} characters in length.");

            RuleFor(v => v.NewCourse.ContentCode)
                .Must(CreateCourseValidationRegex.IsLettersNumbersDashesOnly)
                .WithMessage("Content code can only contain numbers, letters, and/or dashes.")
                .When(v => !string.IsNullOrEmpty(v.NewCourse.ContentCode));

            RuleFor(v => v.NewCourse.ContentCode)
                .Must(CreateCourseValidationRegex.StartsWithLettersOrNumbersOnly)
                .WithMessage("Content code should start with a number or letter.")
                .When(v => !string.IsNullOrEmpty(v.NewCourse.ContentCode));
        };

        // Defines Rules that should only run if Course Name is not empty
        var courseNameNotEmptyRules = () =>
        {
            RuleFor(v => v.NewCourse.CourseName)
                .MaximumLength(CourseNameMaxLength)
                .WithMessage($"Name for the course cannot exceed {CourseNameMaxLength} characters in length.");
        };

        RuleFor(v => v.NewCourse)
            .NotNull()
            .WithMessage("New course is required.")
            .DependentRules(() =>
            {
                RuleFor(v => v.NewCourse.ContentCode)
                    .NotEmpty()
                    .WithMessage("Content code is required.")
                    .DependentRules(contentCodeNotEmptyRules);

                RuleFor(v => v.NewCourse.CourseName)
                    .NotEmpty()
                    .WithMessage("Name for the course is required.")
                    .DependentRules(courseNameNotEmptyRules);

                RuleFor(v => v.NewCourse.OrganizationId)
                    .GreaterThan(0)
                    .WithMessage("Valid organization ID is required.");
            });
    }
}

public static partial class CreateCourseValidationRegex
{
    [GeneratedRegex("^[0-9A-Z-]+$", RegexOptions.IgnoreCase, 1000)]
    private static partial Regex LettersNumbersDashesOnly();
    [GeneratedRegex("\\A[0-9A-Z]+", RegexOptions.IgnoreCase, 1000)]
    private static partial Regex FirstCharacterLettersOrNumbersOnly();

    public static bool IsLettersNumbersDashesOnly(string text) => LettersNumbersDashesOnly().IsMatch(text);

    public static bool StartsWithLettersOrNumbersOnly(string text) =>
        FirstCharacterLettersOrNumbersOnly().IsMatch(text);
}