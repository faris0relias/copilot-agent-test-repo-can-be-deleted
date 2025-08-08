using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class UpdateLessonCommandContractValidator : AbstractValidator<UpdateLessonCommand.Contract>
{
    public UpdateLessonCommandContractValidator()
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

        RuleFor(v => v.LessonDto)
            .NotNull()
            .WithMessage("LessonDto is required.")
            .SetValidator(new UpdateLessonDtoValidator());
    }
}

public class UpdateLessonDtoValidator : LessonBaseValidator<UpdateLessonDto>
{
    public UpdateLessonDtoValidator() { }
}
