using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.App.Features.Course.Validators;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class GetFinalExamByCourseIdQueryContractValidatorTests
{
    private readonly GetFinalExamByCourseIdQueryContractValidator _validator = new();
   
    [Fact]
    public void Validate_CourseIdIsInvalid_ReturnsValidationError()
    {
        var query = new GetFinalExamByCourseIdQuery.Contract
        {
            CourseId = Guid.Empty,
            lang = "en"
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(c => c.CourseId)
            .WithErrorMessage("CourseId is required.");
    }

    [Fact]
    public void Validate_CourseIdIsValid_DoesNotReturnValidationError()
    {
        var query = new GetFinalExamByCourseIdQuery.Contract
        {
            CourseId = Guid.NewGuid(),
            lang = "en"
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(c => c.CourseId);
    }
}
