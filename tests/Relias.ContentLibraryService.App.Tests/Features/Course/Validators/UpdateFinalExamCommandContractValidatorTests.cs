using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Validators;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;
public class UpdateFinalExamCommandContractValidatorTests
{
    private readonly UpdateFinalExamCommandValidator _validator = new();
    [Fact]
    public void Validate_CourseIdIsNull_ReturnsValidationError()
    {
        UpdateFinalExamCommand.Contract command = new()
        {
            CourseId = Guid.Empty,
            OrganizationId = 1,
            UpdatedValues = new Dictionary<string, dynamic>() { { "MinimumPercentageToPass", 30 } },
            UpdatedFinalExam = new FinalExamDto
            {
                MinimumPercentageToPass = 30
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CourseId)
            .WithErrorMessage("Course Id is required.");
    }

    [Fact]
    public void Validate_CourseIdIsValid_DoesNotReturnValidationError()
    {
        UpdateFinalExamCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            OrganizationId = 1,
            UpdatedValues = new Dictionary<string, dynamic>(){{ "MinimumPercentageToPass", 30 }
            },
            UpdatedFinalExam = new FinalExamDto
            {
                MinimumPercentageToPass = 30
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.CourseId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_error_when_Percentage_out_of_range(int percentage)
    {
        Dictionary<string, dynamic> providedValues = new()
        {
            { "MinimumPercentageToPass", 60 }
        };

        UpdateFinalExamCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            OrganizationId = 1,
            UpdatedValues = providedValues,
            UpdatedFinalExam = new FinalExamDto
            {
                MinimumPercentageToPass = percentage
            }

        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.UpdatedFinalExam.MinimumPercentageToPass)
            .WithErrorMessage("Percentage must be between 0 and 100.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Should_Not_error_when_Percentage_in_range(int percentage)
    {
        Dictionary<string, dynamic> providedValues = new Dictionary<string, dynamic>
        {
            { "MinimumPercentageToPass", 60 }
        };
        UpdateFinalExamCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            UpdatedValues = providedValues,
            OrganizationId = 1,
            UpdatedFinalExam = new FinalExamDto
            {
                MinimumPercentageToPass = percentage
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.UpdatedFinalExam.MinimumPercentageToPass);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(61)]
    public void Should_error_when_Duration_minutes_out_of_range(int minutes)
    {
        Dictionary<string, dynamic> providedValues = new Dictionary<string, dynamic>
        {
            { "Minutes", minutes }
        };
        UpdateFinalExamCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            UpdatedValues = providedValues,
            OrganizationId = 1,
            UpdatedFinalExam = new FinalExamDto
            {
                Duration = new()
                {
                    Minutes = minutes
                }
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.UpdatedFinalExam.Duration!.Minutes)
            .WithErrorMessage("Minutes must be between 0 and 60.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    public void Should_Not_error_when_Duration_minutes_in_range(int minutes)
    {
        Dictionary<string, dynamic> providedValues = new Dictionary<string, dynamic>
        {
            { "Minutes", minutes }
        };
        UpdateFinalExamCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            UpdatedValues = providedValues,
            OrganizationId = 1,
            UpdatedFinalExam = new FinalExamDto
            {
                Duration = new()
                {
                    Minutes = minutes
                }
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.UpdatedFinalExam.Duration!.Minutes);
    }

    [Fact]
    public void Should_Not_error_when_Duration_hours_in_range()
    {
        Dictionary<string, dynamic> providedValues = new Dictionary<string, dynamic>
        {
            { "Hours", 10 }
        };
        UpdateFinalExamCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            UpdatedValues = providedValues,
            OrganizationId = 1,
            UpdatedFinalExam = new FinalExamDto
            {
                Duration = new()
                {
                    Hours = 10
                }
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.UpdatedFinalExam.Duration!.Hours);
    }

    [Fact]
    public void Should_error_when_Duration_Hours_LessThanZero()
    {
        Dictionary<string, dynamic> providedValues = new Dictionary<string, dynamic>
        {
            { "Hours", -1 }
        };
        UpdateFinalExamCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            OrganizationId = 1,
            UpdatedValues = providedValues,
            UpdatedFinalExam = new FinalExamDto
            {
                Duration = new()
                {
                    Hours = -1
                }
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.UpdatedFinalExam.Duration!.Hours)
            .WithErrorMessage("Hours must be greater than 0.");
    }

}