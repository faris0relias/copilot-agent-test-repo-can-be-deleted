using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.Domain.Course.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

[ExcludeFromCodeCoverage]
public abstract class LessonBaseValidator<T> : AbstractValidator<T> where T : ILessonValidationModel
{
    protected LessonBaseValidator()
    {
        RuleFor(v => v.LessonType)
            .Must(type => type == "file" || type == "url")
            .WithMessage("Must be a valid lesson type.");

        RuleFor(v => v.Name)
            .NotNull().WithMessage("Name is required.");

        When(v => v.Name != null, () =>
        {
            RuleFor(v => v.Name.En)
                .NotEmpty().WithMessage("Name.En is required.");
        });

        RuleFor(v => v.DurationMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DurationMinutes must be greater than or equal to 0.");

        When(v => v.LessonType == "url", () =>
        {
            RuleFor(v => v.OpensInNewTab)
                .NotNull()
                .WithMessage("OpensInNewTab is required when LessonType is 'url'.");

            RuleFor(v => v.ContentPath)
                .Must(BeValidHttpsUrl)
                .WithMessage("ContentPath must be a valid HTTPS URL.");

            RuleFor(v => v.FormatType)
                .Equal(LessonFormatType.Url)
                .WithMessage("FormatType must be 'Url' when LessonType is 'url'.");
        });

        When(v => v.LessonType == "file", () =>
        {
            RuleFor(v => v.FormatType)
                .Must(format => format == LessonFormatType.Pdf
                             || format == LessonFormatType.Audio
                             || format == LessonFormatType.Video
                             || format == LessonFormatType.Scorm
                             || format == LessonFormatType.Aicc)
                .WithMessage("FormatType must be one of: Pdf, Audio, Video, Scorm, Aicc when LessonType is 'file'.");
        });

        RuleFor(v => v.ContentPath)
            .NotEmpty()
            .WithMessage("ContentPath is required.");
    }

    private static bool BeValidHttpsUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && uriResult.Scheme == Uri.UriSchemeHttps;
    }
}
