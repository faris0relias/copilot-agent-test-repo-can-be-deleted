using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Validators;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class CreateFinalExamCommandContractValidatorTests
{
    private readonly CreateFinalExamCommandContractValidator _validator = new();

    [Fact]
    public void Validate_CourseIdIsNull_ReturnsValidationError()
    {
        CreateFinalExamCommand.Contract command = new()
        {
            CourseId = Guid.Empty,
            OrganizationId = 1
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CourseId)
            .WithErrorMessage("Course Id is required.");
    }

    [Fact]
    public void Validate_CourseIdIsValid_DoesNotReturnValidationError()
    {
        CreateFinalExamCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            OrganizationId = 1 
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.CourseId);
    }
}