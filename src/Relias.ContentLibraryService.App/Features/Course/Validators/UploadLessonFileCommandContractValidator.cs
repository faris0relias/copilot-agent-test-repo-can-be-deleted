using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class UploadLessonFileCommandContractValidator : AbstractValidator<UploadLessonFileCommand.Contract>
{
    private static readonly HashSet<string> ValidExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".mp4", ".pdf", ".zip"
    };

    private static readonly HashSet<char> RestrictedChars = new()
    {
        '<', '>', ':', '"', '|', '?', '*', '/', '\\', '\0'
    };

    private static readonly HashSet<string> ReservedNames = new(
        new[] { "CON", "PRN", "AUX", "NUL" }
        .Concat(Enumerable.Range(1, 9).Select(i => $"COM{i}"))
        .Concat(Enumerable.Range(1, 9).Select(i => $"LPT{i}")),
        StringComparer.OrdinalIgnoreCase
    );

    public UploadLessonFileCommandContractValidator()
    {
        RuleFor(v => v.CourseId)
            .NotEmpty()
            .WithMessage("Course Id is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("Course Id cannot be an empty GUID.");

        RuleFor(v => v.LearningObjectId)
            .NotEmpty()
            .WithMessage("Learning Object Id is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("Learning Object Id cannot be an empty GUID.");

        RuleFor(v => v.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization Id must be greater than 0.");

        RuleFor(v => v.Chunk)
            .NotNull()
            .WithMessage("File chunk is required.");

        RuleFor(v => v.UploadId)
            .NotEmpty()
            .WithMessage("Upload Id is required.");

        RuleFor(v => v.ChunkIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Chunk Index must be greater than or equal to 0.");

        RuleFor(v => v.TotalChunks)
            .GreaterThan(0)
            .WithMessage("Total Chunks must be greater than 0.");

        RuleFor(v => v.FileName)
            .NotEmpty()
            .WithMessage("File Name is required.")
            .Must(fileName => HasValidExtension(fileName))
            .WithMessage("File must have a valid extension (.mp3, .mp4, .pdf, or .zip).")
            .Must(fileName => !ContainsRestrictedCharacters(fileName))
            .WithMessage("File name contains invalid characters.")
            .Must(fileName => !ContainsRestrictedNames(fileName))
            .WithMessage("File name contains restricted system names.")
            .Length(1, 255)
            .WithMessage("File name must be between 1 and 255 characters.");

        RuleFor(v => v)
            .Must(v => v.ChunkIndex < v.TotalChunks)
            .WithMessage("Chunk Index must be less than Total Chunks.");
    }

    private static bool HasValidExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(extension) && ValidExtensions.Contains(extension);
    }

    private static bool ContainsRestrictedCharacters(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return false;
        return fileName.Any(c => RestrictedChars.Contains(c) || char.IsControl(c));
    }

    private static bool ContainsRestrictedNames(string fileName)
    {
        var nameOnly = Path.GetFileNameWithoutExtension(fileName);
        return nameOnly != null && ReservedNames.Contains(nameOnly);
    }
}
