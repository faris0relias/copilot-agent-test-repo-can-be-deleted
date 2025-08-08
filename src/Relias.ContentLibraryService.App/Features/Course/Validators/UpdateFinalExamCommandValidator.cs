using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class UpdateFinalExamCommandValidator : AbstractValidator<UpdateFinalExamCommand.Contract>
{
    public UpdateFinalExamCommandValidator()
    {
        RuleFor(v => v.CourseId)
            .NotEmpty()
            .WithMessage("Course Id is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("Course Id cannot be an empty GUID.");

        RuleFor(v => v.UpdatedFinalExam.MinimumPercentageToPass)
            .InclusiveBetween(0, 100)
            .When(v => v.UpdatedFinalExam.MinimumPercentageToPass != null)
            .WithMessage("Percentage must be between 0 and 100.");

        RuleFor(v => v.UpdatedFinalExam.Duration!.Minutes)
            .InclusiveBetween(0, 60)
            .When(v => v.UpdatedFinalExam.Duration?.Minutes != null)
            .WithMessage("Minutes must be between 0 and 60.");

        RuleFor(v => v.UpdatedFinalExam.Duration!.Hours)
            .GreaterThan(-1)
            .When(v => v.UpdatedFinalExam.Duration?.Hours != null)
            .WithMessage("Hours must be greater than 0.");

    }
}
