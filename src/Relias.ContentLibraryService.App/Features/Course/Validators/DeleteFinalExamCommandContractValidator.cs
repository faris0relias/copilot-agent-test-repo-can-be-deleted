using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class DeleteFinalExamCommandContractValidator : AbstractValidator<DeleteFinalExamCommand.Contract>
{
    public DeleteFinalExamCommandContractValidator()
    {
        RuleFor(x => x.CourseId)
           .NotEmpty().WithMessage("CourseId is required.");
    }
}
