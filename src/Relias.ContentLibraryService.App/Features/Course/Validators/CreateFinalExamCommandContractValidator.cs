using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;
public class CreateFinalExamCommandContractValidator : AbstractValidator<CreateFinalExamCommand.Contract>
{
    public CreateFinalExamCommandContractValidator()
    {
        RuleFor(v => v.CourseId)
        .NotEmpty()
        .WithMessage("Course Id is required.")
        .Must(id => id != Guid.Empty)
        .WithMessage("Course Id cannot be an empty GUID.");
    }
}
