using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Validators;
using Relias.ContentLibraryService.Common.Enums;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class PublishCourseCommandContractValidatorTests
{
    private readonly PublishCourseCommandContractValidator _validator = new();

    private static readonly Guid ValidCourseId = Guid.NewGuid();
    private static readonly DateTime ValidDate = DateTime.UtcNow.AddDays(1);
    private const string ValidUserId = "b198e36f-8e57-43ab-a36e-4a09a5b54215";

    [Fact]
    public void Validate_CourseIdIsEmpty_ReturnsValidationError()
    {
        var command = new PublishCourseCommand.Contract
        {
            CourseId = Guid.Empty,
            PublishCourse = new PublishCourseDto
            {
                StatusId = (int)StatusIdEnums.Published
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CourseId)
              .WithErrorMessage("CourseId is required.");
    }

    [Fact]
    public void Validate_PublishCourseIsNull_ReturnsValidationError()
    {
        var command = new PublishCourseCommand.Contract
        {
            CourseId = ValidCourseId,
            PublishCourse = null!
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.PublishCourse)
              .WithErrorMessage("PublishCourse is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(999)]
    public void Validate_InvalidStatusId_ReturnsValidationError(int invalidStatus)
    {
        var command = new PublishCourseCommand.Contract
        {
            CourseId = ValidCourseId,
            PublishCourse = new PublishCourseDto
            {
                StatusId = invalidStatus
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("PublishCourse.StatusId")
              .WithErrorMessage("StatusId must be ScheduledForPublish (3) or Published (4).");
    }

    [Fact]
    public void Validate_MissingPublishDateWhenScheduled_ReturnsValidationError()
    {
        var command = new PublishCourseCommand.Contract
        {
            CourseId = ValidCourseId,
            PublishCourse = new PublishCourseDto
            {
                StatusId = (int)StatusIdEnums.ScheduledForPublish
                // PublishDate is missing
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("PublishCourse.PublishDate")
              .WithErrorMessage("PublishDate is required when scheduling a course for publish.");
    }

    [Fact]
    public void Validate_InvalidPublishBy_ReturnsValidationError()
    {
        var command = new PublishCourseCommand.Contract
        {
            CourseId = ValidCourseId,
            PublishCourse = new PublishCourseDto
            {
                StatusId = (int)StatusIdEnums.Published,
                PublishBy = "not-a-guid"
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("PublishCourse.PublishBy")
              .WithErrorMessage("PublishBy must be a valid user id.");
    }

    [Fact]
    public void Validate_ValidScheduledPublishCommand_DoesNotReturnErrors()
    {
        var command = new PublishCourseCommand.Contract
        {
            CourseId = ValidCourseId,
            PublishCourse = new PublishCourseDto
            {
                StatusId = (int)StatusIdEnums.ScheduledForPublish,
                PublishDate = ValidDate,
                PublishBy = ValidUserId
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidImmediatePublishCommand_DoesNotReturnErrors()
    {
        var command = new PublishCourseCommand.Contract
        {
            CourseId = ValidCourseId,
            PublishCourse = new PublishCourseDto
            {
                StatusId = (int)StatusIdEnums.Published,
                PublishDate = null,
                PublishBy = ValidUserId
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
