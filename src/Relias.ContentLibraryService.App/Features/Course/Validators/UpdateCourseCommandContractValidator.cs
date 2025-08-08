using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using System.Text.RegularExpressions;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class UpdateCourseCommandContractValidator : AbstractValidator<UpdateCourseCommand.Contract>
{   
    private const int ContentCodeMaxLength = 100;   

    public UpdateCourseCommandContractValidator()
    {
        // Defines Rules that should only run if Content Code is not empty
        var contentCodeNotEmptyRules = () =>
        {
            RuleFor(v => v.UpdatedCourse.ContentCode)
                .MaximumLength(ContentCodeMaxLength)
                .WithMessage($"Content code can not exceed {ContentCodeMaxLength} characters in length.");

            RuleFor(v => v.UpdatedCourse.ContentCode)
                .Must(UpdateCourseValidationRegex.IsLettersNumbersDashesOnly)
                .WithMessage("Content code can only contain numbers, letters, and/or dashes.")
                .When(v => !string.IsNullOrEmpty(v.UpdatedCourse.ContentCode));

            RuleFor(v => v.UpdatedCourse.ContentCode)
                .Must(UpdateCourseValidationRegex.StartsWithLettersOrNumbersOnly)
                .WithMessage("Content code should start with a number or letter.")
                .When(v => !string.IsNullOrEmpty(v.UpdatedCourse.ContentCode));
        };

        RuleFor(v => v.UpdatedCourse)
          .NotNull()
          .WithMessage("Update course is required.")
          .DependentRules(() =>
          {
              RuleFor(v => v.UpdatedCourse.ContentCode)
                  .NotEmpty()
                  .WithMessage("Content Code is required.")
                  .DependentRules(contentCodeNotEmptyRules);
          });           
    }   
}

public static partial class UpdateCourseValidationRegex
{
    [GeneratedRegex("^[0-9A-Z-]+$", RegexOptions.IgnoreCase, 1000)]
    private static partial Regex LettersNumbersDashesOnly();
    [GeneratedRegex("\\A[0-9A-Z]+", RegexOptions.IgnoreCase, 1000)]
    private static partial Regex FirstCharacterLettersOrNumbersOnly();

    public static bool IsLettersNumbersDashesOnly(string text) => LettersNumbersDashesOnly().IsMatch(text);

    public static bool StartsWithLettersOrNumbersOnly(string text) =>
        FirstCharacterLettersOrNumbersOnly().IsMatch(text);
}