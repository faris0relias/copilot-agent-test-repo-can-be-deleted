using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.Enums;


namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class UpdateFinalExamQuestionCommandContractValidator : AbstractValidator<UpdateFinalExamQuestionCommand.Contract>
{
    public UpdateFinalExamQuestionCommandContractValidator()
    {
        RuleFor(v => v.CourseId)
            .NotEmpty()
            .WithMessage("CourseId is required.");

        RuleFor(v => v.ExamId)
            .NotEmpty()
            .WithMessage("ExamId is required.");

        RuleFor(v => v.Updates)
            .NotNull()
            .WithMessage("Updates are required.")
            .DependentRules(() =>
            {

                // Validate Question Update action
                When(v => v.Updates.QuestionUpdate != null, () =>
                {
                    RuleFor(v => v.Updates.QuestionUpdate)
                        .SetValidator(new QuestionUpdateDtoValidator()!);

                    RuleFor(v => v.Updates.QuestionUpdate!.QuestionType)
                    .IsInEnum()
                    .When(x => x.Updates.QuestionUpdate!.QuestionType != null)
                    .WithMessage("Question type must be valid");
                });

                // Validate Question option Updates for correct answer and options of question update
                When(v => v.Updates.QuestionUpdate != null && v.Updates.QuestionUpdate.QuestionType == QuestionType.SingleSelect && v.Updates.QuestionUpdate.Action == PatchAction.Add, () =>
                {
                    RuleFor(v => v.Updates.QuestionOptionsUpdate)
                   .NotEmpty()
                   .Must(list => list != null && list.Count(option => option.IsCorrect == true) == 1)
                   .WithMessage("Question must contain only one correct answer.")
                   .Must(list => list != null && list.Count > 1 && list.Count <= 50)
                   .WithMessage("Question must contain at least 2 options and a maximum of 50.");

                });

                // Validate Question option Updates for correct answer and options of question update
                When(v => v.Updates.QuestionUpdate != null && v.Updates.QuestionUpdate.QuestionType == QuestionType.MultiSelect && v.Updates.QuestionUpdate.Action == PatchAction.Add, () =>
                {
                    RuleFor(v => v.Updates.QuestionOptionsUpdate)
                   .NotEmpty()
                   .Must(list => list != null && list.Any(Item => Item.IsCorrect == true))
                   .WithMessage("Question must contain at least one correct answer.")
                   .Must(list => list != null && list.Count > 1 && list.Count <= 50)
                   .WithMessage("Question must contain at least 2 options and a maximum of 50.");

                });


                // Validate Question Options Updates for each list item
                When(v => v.Updates.QuestionOptionsUpdate != null, () =>
                {
                    // Validate each option in the QuestionOptions Updates collection
                    RuleForEach(v => v.Updates.QuestionOptionsUpdate)
                        .SetValidator(new QuestionOptionsUpdateDtoValidator()!);
                });
            });
    }
}

public class QuestionUpdateDtoValidator : AbstractValidator<FinalExamUpdateQuestionDto>
{
    public QuestionUpdateDtoValidator()
    {
        RuleFor(v => v.Action)
            .IsInEnum()
            .WithMessage("Action must be either 'Add', 'Remove', or 'Replace'.");

        When(v => v.Action == PatchAction.Add || v.Action == PatchAction.Replace, () =>
        {
            RuleFor(v => v.QuestionText)
                .NotNull()
                .WithMessage("QuestionText is required for question.");
            When(v => v.QuestionText != null, () =>
            {
                RuleFor(v => v.QuestionText!.En)
                    .NotEmpty()
                    .WithMessage("QuestionText.En cannot be empty.");
            });
        });
    }
}

public class QuestionOptionsUpdateDtoValidator : AbstractValidator<FinalExamQuestionUpdateOptionDto>
{
    public QuestionOptionsUpdateDtoValidator()
    {
        RuleFor(v => v.Action)
            .IsInEnum()
            .WithMessage("Action must be either 'Add', 'Remove', or 'Replace'.");

        When(v => v.Action == PatchAction.Add || v.Action == PatchAction.Replace, () =>
        {
            RuleFor(v => v.OptionText)
                .NotNull()
                .WithMessage("OptionText is required for question option.");
            When(v => v.OptionText != null, () =>
            {
                RuleFor(v => v.OptionText!.En)
                    .NotEmpty()
                    .WithMessage("OptionText.En cannot be empty.");
            });
        });

        When(v => v.Action == PatchAction.Remove, () =>
        {
            RuleFor(v => v.OptionId)
                .NotEmpty()
                .WithMessage("OptionId is required for question option delete.");
        });

    }
}