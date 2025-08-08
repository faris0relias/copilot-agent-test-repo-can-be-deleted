using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Validators;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class DeleteLessonCommandContractValidatorTests
{
    private readonly DeleteLessonCommandContractValidator _validator = new();

    [Fact]
    public void Validate_ValidContract_ShouldNotHaveErrors()
    {
        // Arrange
        var contract = new DeleteLessonCommand.Contract(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1);

        // Act & Assert
        var result = _validator.TestValidate(contract);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyCourseId_ShouldHaveError()
    {
        // Arrange
        var contract = new DeleteLessonCommand.Contract(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), 0);

        // Act & Assert
        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(x => x.CourseId)
              .WithErrorMessage("CourseId is required.");
    }

    [Fact]
    public void Validate_EmptySectionId_ShouldHaveError()
    {
        // Arrange
        var contract = new DeleteLessonCommand.Contract(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 0);

        // Act & Assert
        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(x => x.SectionId)
              .WithErrorMessage("SectionId is required.");
    }

    [Fact]
    public void Validate_EmptyLearningObjectId_ShouldHaveError()
    {
        // Arrange
        var contract = new DeleteLessonCommand.Contract(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 0);

        // Act & Assert
        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(x => x.LearningObjectId)
              .WithErrorMessage("LearningObjectId is required.");
    }

    [Fact]
    public void Validate_InvalidOrgId_ShouldHaveError()
    {
        // Arrange
        var contract = new DeleteLessonCommand.Contract(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0);

        // Act & Assert
        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(x => x.OrgId)
              .WithErrorMessage("Organization ID must be a valid id.");
    }
}
