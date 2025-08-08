using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class DeleteCourseCommandContractValidator : AbstractValidator<DeleteCourseCommand.Contract>
{
    public DeleteCourseCommandContractValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");
    }
}