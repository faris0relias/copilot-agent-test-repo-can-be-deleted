using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Validators;
using Relias.ContentLibraryService.App.Features.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.Enums;


namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class UpdateFinalExamQuestionCommandContractValidatorTests
{
    private readonly UpdateFinalExamQuestionCommandContractValidator _validator = new();

    [Fact]
    public void Validate_WhenCourseIdIsEmptyGuid_ShouldReturnValidationError()
    {
        // Arrange
        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.Empty,
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CourseId)
            .WithErrorMessage("CourseId is required.");
    }
    [Fact]
    public void Validator_Should_Fail_When_QuestionType_IsInvalidEnum()
    {
        

        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto
            {
                QuestionUpdate = new FinalExamUpdateQuestionDto
                {

                    Action = (PatchAction)999, // Invalid action
                    QuestionText = new LocalizedStringDto { En = "Question 1" },
                    QuestionType = (QuestionType)99 // Invalid enum
                }                    
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Updates.QuestionUpdate!.QuestionType)
            .WithErrorMessage("Question type must be valid");
    }

    [Fact]
    public void Validate_WhenExamIdIsEmptyGuid_ShouldReturnValidationError()
    {
        // Arrange
        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.Empty,
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ExamId)
            .WithErrorMessage("ExamId is required.");
    }

    [Fact]
    public void Validate_WhenUpdatesIsNull_ShouldReturnValidationError()
    {
        // Arrange
        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = null!
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Updates)
            .WithErrorMessage("Updates are required.");
    }

    [Fact]
    public void Validate_WhenActionIsNotInEnumForQuestionUpdate_ShouldReturnValidationError()
    {

        // Arrange
        List<FinalExamQuestionUpdateOptionDto> QuestionOptionsUpdateList = new List<FinalExamQuestionUpdateOptionDto>()
        {
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = (PatchAction)999, // Invalid action
                OptionText = new LocalizedStringDto { En = "Option 1" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 1" },
                IsCorrect = true
            },
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = (PatchAction)999, // Invalid action
                OptionText = new LocalizedStringDto { En = "Option 2" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 2" },
                IsCorrect = false
            }
        };

        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto
            {
                QuestionUpdate = new FinalExamUpdateQuestionDto
                {
                    Action = (PatchAction)999, // Invalid action
                    QuestionText = new LocalizedStringDto { En = "Question 1" },
                    QuestionType = QuestionType.SingleSelect
                },
                QuestionOptionsUpdate = QuestionOptionsUpdateList
            }

        };
        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result
          .ShouldHaveValidationErrorFor("Updates.QuestionUpdate.Action")
          .WithErrorMessage("Action must be either 'Add', 'Remove', or 'Replace'.");
    }

    [Theory]
    [InlineData(PatchAction.Add)]
    [InlineData(PatchAction.Replace)]
    public void Validate_WhenQuestionTextIsNullForAddOrReplaceAction_ShouldReturnValidationError(PatchAction action)
    {
        // Arrange
        List<FinalExamQuestionUpdateOptionDto> QuestionOptionsUpdateList = new List<FinalExamQuestionUpdateOptionDto>()
        {
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = (PatchAction)999, // Invalid action
                OptionText = new LocalizedStringDto { En = "Option 1" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 1" },
                IsCorrect = true
            },
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = (PatchAction)999, // Invalid action
                OptionText = new LocalizedStringDto { En = "Option 2" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 2" },
                IsCorrect = false
            }
        };

        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto
            {

                QuestionUpdate = new FinalExamUpdateQuestionDto
                {
                    Action = action, 
                    QuestionText = null,
                    QuestionType = QuestionType.SingleSelect
                },
                QuestionOptionsUpdate = QuestionOptionsUpdateList
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Updates.QuestionUpdate.QuestionText")
            .WithErrorMessage("QuestionText is required for question.");
    }

    [Theory]
    [InlineData(PatchAction.Add)]
    [InlineData(PatchAction.Replace)]
    public void Validate_WhenQuestionTextEnIsEmpty_ShouldReturnValidationError(PatchAction action)
    {
        // Arrange
        List<FinalExamQuestionUpdateOptionDto> QuestionOptionsUpdateList = new List<FinalExamQuestionUpdateOptionDto>()
        {
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = (PatchAction)999, // Invalid action
                OptionText = new LocalizedStringDto { En = "Option 1" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 1" },
                IsCorrect = true
            },
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = (PatchAction)999, // Invalid action
                OptionText = new LocalizedStringDto { En = "Option 2" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 2" },
                IsCorrect = false
            }
        };

        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto
            {
                QuestionUpdate = new FinalExamUpdateQuestionDto
                {
                    Action = action,
                    QuestionText = new LocalizedStringDto { En = string.Empty },
                    QuestionType = QuestionType.MultiSelect
                },
                QuestionOptionsUpdate = QuestionOptionsUpdateList
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Updates.QuestionUpdate.QuestionText.En")
            .WithErrorMessage("QuestionText.En cannot be empty.");
    }

    [Fact]
    public void Validate_WhenActionIsNotInEnumForQuestionOptionsUpdate_ShouldReturnValidationError()
    {
        // Arrange
        List<FinalExamQuestionUpdateOptionDto> QuestionOptionsUpdateList = new List<FinalExamQuestionUpdateOptionDto>()
        {
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = (PatchAction)999, // Invalid action
                OptionText = new LocalizedStringDto { En = "Option 1" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 1" },
                IsCorrect = true
            },
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = (PatchAction)999, // Invalid action
                OptionText = new LocalizedStringDto { En = "Option 2" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 2" },
                IsCorrect = false
            }
        };

        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto
            {
                QuestionOptionsUpdate = QuestionOptionsUpdateList

            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Updates.QuestionOptionsUpdate[0].Action")
            .WithErrorMessage("Action must be either 'Add', 'Remove', or 'Replace'.");
    }

    [Fact]
    public void Validate_WhenOptionTextIsNullInQuestionOptionsUpdate_ShouldReturnValidationError()
    {
        // Arrange
        List<FinalExamQuestionUpdateOptionDto> QuestionOptionsUpdateList = new List<FinalExamQuestionUpdateOptionDto>();
        QuestionOptionsUpdateList.Add(new FinalExamQuestionUpdateOptionDto
        {
            OptionId = Guid.NewGuid(),
            Action = PatchAction.Replace, 
            OptionText = null,
            ResponseFeedback = new LocalizedStringDto { En = "Response 1" },
            IsCorrect = true
        });
        QuestionOptionsUpdateList.Add(new FinalExamQuestionUpdateOptionDto
        {
            OptionId = Guid.NewGuid(),
            Action = PatchAction.Replace, 
            OptionText = new LocalizedStringDto { En = "Option 2" },
            ResponseFeedback = new LocalizedStringDto { En = "Response 2" },
            IsCorrect = false
        });

        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto
            {
                QuestionOptionsUpdate = QuestionOptionsUpdateList
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Updates.QuestionOptionsUpdate[0].OptionText")
            .WithErrorMessage("OptionText is required for question option.");
    }

    [Fact]
    public void Validate_WhenOptionTextEnIsEmptyInQuestionOptionsUpdate_ShouldReturnValidationError()
    {
        // Arrange
        List<FinalExamQuestionUpdateOptionDto> QuestionOptionsUpdateList = new List<FinalExamQuestionUpdateOptionDto>()
        {
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = PatchAction.Replace,
                OptionText = new LocalizedStringDto { En = string.Empty },
                ResponseFeedback = new LocalizedStringDto { En = "Response 1" },
                IsCorrect = true
            },
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = PatchAction.Replace,
                OptionText = new LocalizedStringDto { En = string.Empty },
                ResponseFeedback = new LocalizedStringDto { En = "Response 2" },
                IsCorrect = false
            }

        };
        
        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto
            {
                QuestionOptionsUpdate = QuestionOptionsUpdateList
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Updates.QuestionOptionsUpdate[0].OptionText.En")
            .WithErrorMessage("OptionText.En cannot be empty.");
    }

    
    [Fact]
    public void Validate_WhenQuestionOptionMustTwoOptionsOnSelectQuetionType_ShouldReturnValidationError()
    {
        // Arrange
        List<FinalExamQuestionUpdateOptionDto> QuestionOptionsUpdateList = new List<FinalExamQuestionUpdateOptionDto>()
        {
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = PatchAction.Add,
                OptionText = new LocalizedStringDto { En = "Option 1" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 1" },
                IsCorrect = true
            }
        };
        
        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto
            {
                QuestionUpdate = new FinalExamUpdateQuestionDto
                {
                    Action = PatchAction.Add,
                    QuestionText = new LocalizedStringDto { En = "Question 1" },
                    QuestionType = QuestionType.SingleSelect
                },
                QuestionOptionsUpdate = QuestionOptionsUpdateList
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Updates.QuestionOptionsUpdate")
            .WithErrorMessage("Question must contain at least 2 options and a maximum of 50.");
    }


    [Fact]
   
    public void Validate_WhenQuestionOptionMustOneCorrectAnswer_ShouldReturnValidationError()
    {
        // Arrange
        List<FinalExamQuestionUpdateOptionDto> QuestionOptionsUpdateList = new List<FinalExamQuestionUpdateOptionDto>()
        {
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = PatchAction.Add,
                OptionText = new LocalizedStringDto { En = "Option 1" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 1" },
                IsCorrect = false
            },
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = PatchAction.Add,
                OptionText = new LocalizedStringDto { En = "Option 2" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 2" },
                IsCorrect = false
            },
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = PatchAction.Add,
                OptionText = new LocalizedStringDto { En = "Option 3" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 3" },
                IsCorrect = false
            }
        };
        
        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto
            {
                QuestionUpdate = new FinalExamUpdateQuestionDto
                {
                    Action = PatchAction.Add,
                    QuestionText = new LocalizedStringDto { En = "Question 1" },
                    QuestionType = QuestionType.MultiSelect
                },
                QuestionOptionsUpdate = QuestionOptionsUpdateList
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Updates.QuestionOptionsUpdate")
            .WithErrorMessage("Question must contain at least one correct answer.");
    }

    [Fact]
    public void Validate_WhenQuestionOptionIdEmptyGuid_ShouldReturnValidationError()
    {
        // Arrange
        List<FinalExamQuestionUpdateOptionDto> QuestionOptionsUpdateList = new List<FinalExamQuestionUpdateOptionDto>()
        {
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.Empty,
                Action = PatchAction.Remove,
               
            }
        };

        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto
            {
                QuestionOptionsUpdate = QuestionOptionsUpdateList
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Updates.QuestionOptionsUpdate[0].OptionId")
            .WithErrorMessage("OptionId is required for question option delete.");
    }

    [Theory]
    [InlineData(PatchAction.Add)]
    [InlineData(PatchAction.Replace)]
    public void Validate_WhenAllPropertiesAreValid_ShouldNotReturnValidationError(PatchAction action)
    {
        List<FinalExamQuestionUpdateOptionDto> QuestionOptionsUpdateList = new List<FinalExamQuestionUpdateOptionDto>()
        {
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = action,
                OptionText = new LocalizedStringDto { En = "Option 1" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 1" },
                IsCorrect = true
            },
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = action,
                OptionText = new LocalizedStringDto { En = "Option 2" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 2" },
                IsCorrect = false
            },
            new FinalExamQuestionUpdateOptionDto
            {
                OptionId = Guid.NewGuid(),
                Action = action,
                OptionText = new LocalizedStringDto { En = "Option 3" },
                ResponseFeedback = new LocalizedStringDto { En = "Response 3" },
                IsCorrect = false
            }
        };
        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            Updates = new FinalExamQuestionPatchDto
            {
                QuestionUpdate = new FinalExamUpdateQuestionDto
                {
                    Action = action,
                    QuestionText = new LocalizedStringDto { En = "Question 1" },
                    QuestionType = QuestionType.MultiSelect
                },
                QuestionOptionsUpdate = QuestionOptionsUpdateList
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}