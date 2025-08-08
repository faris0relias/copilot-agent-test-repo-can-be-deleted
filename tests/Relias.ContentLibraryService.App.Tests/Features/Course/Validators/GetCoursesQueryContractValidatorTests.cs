using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.App.Features.Course.Validators;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class GetCoursesQueryContractValidatorTests
{
    private readonly GetCoursesQueryContractValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_OrganizationIdIsInvalid_ReturnsValidationError(int organizationId)
    {
        var query = new GetCoursesQuery.Contract
        {           
            OrganizationId = organizationId
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(c => c.OrganizationId)
            .WithErrorMessage("A valid organization id is required");
    }

    [Fact]
    public void Validate_OrganizationIdIsValid_DoesNotReturnValidationError()
    {
        var query = new GetCoursesQuery.Contract
        {
            OrganizationId = 1
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(c => c.OrganizationId);
    }
}

