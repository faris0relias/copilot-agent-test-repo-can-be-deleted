using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Helpers;
using Operation = Microsoft.AspNetCore.JsonPatch.Operations.Operation;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class UpdateLearningContentCommandContractValidator : AbstractValidator<UpdateLearningContentCommand.Contract>
{
    public UpdateLearningContentCommandContractValidator()
    {
        RuleFor(v => v.CourseId)
            .NotEmpty()
            .WithMessage("CourseId is required.");

        RuleFor(v => v.Updates)
            .NotNull()
            .WithMessage("Updates are required.")
            .DependentRules(() =>
            {
                RuleForEach(v => v.Updates.Operations)
                    .SetValidator(new JsonPatchDocumentValidator());
            });
    }
}

public class JsonPatchDocumentValidator : AbstractValidator<Operation>
{
    public JsonPatchDocumentValidator()
    {
        RuleFor(v => v.op)
            .NotEmpty()
            .WithMessage("Valid operation type: 'add', 'replace', 'move', or 'remove' is required.");

        RuleFor(v => v.path)
            .NotEmpty()
            .WithMessage("A valid path is required to update target location.")
            .DependentRules(() =>
            {
                When(v => v.OperationType == OperationType.Move, () =>
                {
                    RuleFor(v => v.from)
                        .NotNull()
                        .NotEmpty()
                        .WithMessage("From parameter can not be null or empty.");
                });

            });

        When(v => v.OperationType == OperationType.Add || v.OperationType == OperationType.Replace, () =>
        {
            RuleFor(v => v.value)
                .NotNull()
                .WithMessage("Value is required.");
        });
    }
}

public class LearningContentDtoValidator : AbstractValidator<LearningContentDto>
{
    public LearningContentDtoValidator()
    {
        RuleForEach(v => v.Sections)
            .SetValidator(new SectionUpdateDtoValidator());

        RuleFor(v => v.Sections)
            .NotEmpty()
            .Custom((sections, context) =>
            {
                List<ValidationFailure> failures = SectionNameValidatorHelper.GetSectionNameErrors(sections);
                if (failures.Any())
                {
                    foreach (ValidationFailure failure in failures)
                    {
                        context.AddFailure(failure);
                    }
                }
            });
    }
}

public class SectionUpdateDtoValidator : AbstractValidator<LearningContentSectionDto>
{
    public SectionUpdateDtoValidator()
    {
        RuleFor(v => v.Name)
            .NotNull()
            .WithMessage("Name is required for section update.");

        RuleFor(v => v.SectionId)
            .NotEmpty()
            .WithMessage("SectionId is required.");

        When(v => v.LearningObjects.Count > 0, () =>
        {
            RuleForEach(s => s.LearningObjects)
                .SetValidator(new LearningObjectUpdateDtoValidator());
        });
    }
}

public class LearningObjectUpdateDtoValidator : AbstractValidator<LearningObjectDto>
{
    public LearningObjectUpdateDtoValidator()
    {
        RuleFor(v => v.LearningObjectType)
            .IsInEnum()
            .WithMessage("A valid Learning Object Type is required.");
    }
}

// For use in future ticket to validate LearningObjectType.Lesson
// TODO: Update Validation for use to validate LearningObjectType.Lesson

//public class LearningObjectLessonDtoValidator : AbstractValidator<LessonDto>
//{
//    public LearningObjectLessonDtoValidator()
//    {
//        RuleFor(v => v.LessonType)
//            .NotNull()
//            .WithMessage("Lesson type is required for learning object type 'Lesson'");

//        RuleFor(v => v.Name)
//            .NotNull()
//            .WithMessage("Name is required for learning object type 'Lesson'.");

//        RuleFor(v => v.Name!.En)
//            .NotEmpty()
//            .WithMessage("Name.En is required for learning object type 'Lesson'.");

//        RuleFor(v => v.DurationMinutes)
//            .NotNull()
//            .WithMessage("DurationMinutes must provided for learning object type 'Lesson'.");

//        RuleFor(v => v.DurationMinutes)
//            .GreaterThan(0)
//            .WithMessage("DurationMinutes must be greater than 0 for learning object type 'Lesson'.");
//    }
//}
