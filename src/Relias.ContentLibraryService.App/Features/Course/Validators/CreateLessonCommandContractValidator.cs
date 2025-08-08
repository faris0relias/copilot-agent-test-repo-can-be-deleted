using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class CreateLessonCommandContractValidator : AbstractValidator<CreateLessonCommand.Contract>
{
    public CreateLessonCommandContractValidator()
    {
        RuleFor(v => v.CourseId)
            .NotEmpty()
            .WithMessage("CourseId is required.");

        RuleFor(v => v.SectionId)
            .NotEmpty()
            .WithMessage("SectionId is required.");

        RuleFor(v => v.LessonDto)
            .NotNull()
            .WithMessage("LessonDto is required.")
            .SetValidator(new CreateLessonDtoValidator());
    }
}

public class CreateLessonDtoValidator : LessonBaseValidator<CreateLessonDto>
{
    public CreateLessonDtoValidator() { }
}
