using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class DeleteLessonCommandContractValidator : AbstractValidator<DeleteLessonCommand.Contract>
{
    public DeleteLessonCommandContractValidator()
    {
        RuleFor(v => v.CourseId)
            .NotEmpty()
            .WithMessage("CourseId is required.");

        RuleFor(v => v.SectionId)
            .NotEmpty()
            .WithMessage("SectionId is required.");

        RuleFor(v => v.LearningObjectId)
            .NotEmpty()
            .WithMessage("LearningObjectId is required.");
        
        RuleFor(v => v.OrgId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be a valid id.");
    }
}
