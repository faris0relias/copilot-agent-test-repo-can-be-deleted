using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Validators;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class DeleteCourseCommandContractValidatorTests
{
    private readonly DeleteCourseCommandContractValidator _validator = new();
    private const int OrganizationId = 8;

    [Fact]
    public void Validate_CourseIdIsInvalid_ReturnsValidationError()
    {
        DeleteCourseCommand.Contract command = new(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CourseId)
            .WithErrorMessage("CourseId is required.");
    }

    [Fact]
    public void Validate_CourseIdIsValid_DoesNotReturnValidationError()
    {
        DeleteCourseCommand.Contract command = new(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.CourseId);
    }
}