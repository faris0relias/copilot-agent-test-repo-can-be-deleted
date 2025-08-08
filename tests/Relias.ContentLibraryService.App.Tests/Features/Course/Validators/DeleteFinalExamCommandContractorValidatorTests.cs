using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Validators;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class DeleteFinalExamCommandContractorValidatorTests
{
    private readonly DeleteFinalExamCommandContractValidator _validator = new();
    
    [Fact]
    public void Validate_CourseIdIsInvalid_ReturnsValidationError()
    {
        DeleteFinalExamCommand.Contract command = new(
            Guid.Empty
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CourseId)
            .WithErrorMessage("CourseId is required.");
    }

    [Fact]
    public void Validate_CourseIdIsValid_DoesNotReturnValidationError()
    {
        DeleteFinalExamCommand.Contract command = new(
            Guid.NewGuid()
        );

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.CourseId);
    }
}
