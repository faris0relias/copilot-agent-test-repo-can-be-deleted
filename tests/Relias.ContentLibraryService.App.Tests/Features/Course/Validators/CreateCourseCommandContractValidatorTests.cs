using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Validators;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class CreateCourseCommandContractValidatorTests
{
    private readonly CreateCourseCommandContractValidator _validator = new();
    private const int ValidOrgId = 1;
    private const string ValidContentCode = "ABC-123";
    private const string ValidCourseName = "Example";

    [Fact]
    public void Validate_NewCourseIsNull_ReturnsValidationError()
    {
        CreateCourseCommand.Contract command = new() { NewCourse = null! };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewCourse)
            .WithErrorMessage("New course is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ContentCodeIsNullOrEmpty_ReturnsValidationError(string? emptyContentCode)
    {
        CreateCourseCommand.Contract command = new()
        {
            NewCourse = new CreateCourseDto
            {
                OrganizationId = ValidOrgId,
                CourseName = ValidCourseName,
                ContentCode = emptyContentCode!
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewCourse.ContentCode)
            .WithErrorMessage("Content code is required.");
    }

    [Fact]
    public void Validate_ContentCodeGreaterThan100Characters_ReturnsValidationError()
    {
        string longContentCode = new('a', 101);
        CreateCourseCommand.Contract command = new()
        {
            NewCourse = new CreateCourseDto
            {
                OrganizationId = ValidOrgId,
                CourseName = ValidCourseName,
                ContentCode = longContentCode
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewCourse.ContentCode)
            .WithErrorMessage("Content code can not exceed 100 characters in length.");
    }

    [Theory]
    [InlineData("ABC!123")]
    [InlineData("1!?*.A-")]
    public void Validate_ContentCodeIncludesNonAlphaNumericDashCharacters_ReturnsValidationError(string invalidContentCode)
    {
        CreateCourseCommand.Contract command = new()
        {
            NewCourse = new CreateCourseDto
            {
                OrganizationId = ValidOrgId,
                CourseName = ValidCourseName,
                ContentCode = invalidContentCode
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewCourse.ContentCode)
            .WithErrorMessage("Content code can only contain numbers, letters, and/or dashes.");
    }

    [Theory]
    [InlineData("-ABC123")]
    [InlineData("!ABC123")]
    public void Validate_ContentCodeStartsWithNonAlphaNumericCharacter_ReturnsValidationError(string invalidContentCode)
    {
        CreateCourseCommand.Contract command = new()
        {
            NewCourse = new CreateCourseDto
            {
                OrganizationId = ValidOrgId,
                CourseName = ValidCourseName,
                ContentCode = invalidContentCode
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewCourse.ContentCode)
            .WithErrorMessage("Content code should start with a number or letter.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_CourseNameIsNullOrEmpty_ReturnsValidationError(string? emptyCourseName)
    {
        CreateCourseCommand.Contract command = new()
        {
            NewCourse = new CreateCourseDto
            {
                OrganizationId = ValidOrgId,
                CourseName = emptyCourseName!,
                ContentCode = ValidContentCode
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewCourse.CourseName)
            .WithErrorMessage("Name for the course is required.");
    }

    [Fact]
    public void Validate_CourseNameGreaterThan256Characters_ReturnsValidationError()
    {
        string longCourseName = new('a', 501);
        CreateCourseCommand.Contract command = new()
        {
            NewCourse = new CreateCourseDto
            {
                OrganizationId = ValidOrgId,
                CourseName = longCourseName,
                ContentCode = ValidContentCode
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewCourse.CourseName)
            .WithErrorMessage("Name for the course cannot exceed 500 characters in length.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_OrganizationIdIsInvalid_ReturnsValidationError(int invalidOrgId)
    {
        CreateCourseCommand.Contract command = new()
        {
            NewCourse = new CreateCourseDto
            {
                OrganizationId = invalidOrgId,
                CourseName = ValidCourseName,
                ContentCode = ValidContentCode
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewCourse.OrganizationId)
            .WithErrorMessage("Valid organization ID is required.");
    }

    [Fact]
    public void Validate_CommandIsValid_DoesNotReturnValidationError()
    {
        CreateCourseCommand.Contract command = new()
        {
            NewCourse = new CreateCourseDto
            {
                OrganizationId = ValidOrgId,
                CourseName = ValidCourseName,
                ContentCode = ValidContentCode
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.NewCourse);
        result.ShouldNotHaveValidationErrorFor(c => c.NewCourse.OrganizationId);
        result.ShouldNotHaveValidationErrorFor(c => c.NewCourse.CourseName);
        result.ShouldNotHaveValidationErrorFor(c => c.NewCourse.ContentCode);
    }
}