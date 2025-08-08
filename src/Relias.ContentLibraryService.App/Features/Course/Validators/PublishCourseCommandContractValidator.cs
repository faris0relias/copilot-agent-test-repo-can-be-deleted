using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.Common.Enums;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class PublishCourseCommandContractValidator : AbstractValidator<PublishCourseCommand.Contract>
{
    public PublishCourseCommandContractValidator()
    {
        RuleFor(v => v.CourseId)
            .NotEmpty()
            .WithMessage("CourseId is required.");

        RuleFor(v => v.PublishCourse)
            .NotNull()
            .WithMessage("PublishCourse is required.")
            .DependentRules(() =>
            {
                RuleFor(v => v.PublishCourse.StatusId)
                    .Must(status => status == (int)StatusIdEnums.ScheduledForPublish || status == (int)StatusIdEnums.Published)
                    .WithMessage("StatusId must be ScheduledForPublish (3) or Published (4).");

                RuleFor(v => v.PublishCourse.PublishDate)
                    .NotNull()
                    .WithMessage("PublishDate is required when scheduling a course for publish.")
                    .When(v => v.PublishCourse.StatusId == (int)StatusIdEnums.ScheduledForPublish);

                RuleFor(v => v.PublishCourse.PublishDate!.Value)
                    .Must(date => date >= DateTime.UtcNow)
                    .WithMessage("Scheduled publish date cannot be in the past.")
                    .When(v => v.PublishCourse.StatusId == (int)StatusIdEnums.ScheduledForPublish && v.PublishCourse.PublishDate != null);

                RuleFor(v => v.PublishCourse.PublishBy)
                    .Must(value => string.IsNullOrEmpty(value) || Guid.TryParse(value, out _))
                    .WithMessage("PublishBy must be a valid user id.");
            });
    }
}
