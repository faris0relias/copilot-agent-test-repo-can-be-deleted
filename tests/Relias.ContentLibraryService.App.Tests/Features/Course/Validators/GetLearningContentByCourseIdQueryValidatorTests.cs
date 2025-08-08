using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.App.Features.Course.Validators;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class GetLearningContentByCourseIdQueryValidatorTests
{
    private readonly GetLearningContentByCourseIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenCourseIdIsEmpty_ShouldReturnValidationError()
    {
        var query = new GetLearningContentByCourseIdQuery.Contract
        {
            CourseId = Guid.Empty
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.CourseId)
            .WithErrorMessage("CourseId is required.");
    }

    [Fact]
    public void Validate_WhenCourseIdIsValid_ShouldNotReturnValidationError()
    {
        var query = new GetLearningContentByCourseIdQuery.Contract
        {
            CourseId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(q => q.CourseId);
    }
}
