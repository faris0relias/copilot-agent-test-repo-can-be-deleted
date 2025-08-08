using FluentValidation.TestHelper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Validators;
using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class UpdateLearningContentCommandContractValidatorTests
{
    private readonly UpdateLearningContentCommandContractValidator _validator = new();
    private readonly LearningContentDtoValidator _learningContentDtoValidator = new();
    private static readonly Guid ValidSectionId = Guid.NewGuid();
    private const string ValidSectionName = "Test Section";
    private const string AddOperation = "add";
    private const string ReplaceOperation = "replace";

    #region Command Validations
    [Fact]
    public void Validate_WhenCourseIdIsEmptyGuid_ShouldReturnValidationError()
    {
        var command = new UpdateLearningContentCommand.Contract
        {
            CourseId = Guid.Empty,
            Updates = new JsonPatchDocument()
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CourseId)
            .WithErrorMessage("CourseId is required.");
    }

    [Fact]
    public void Validate_WhenUpdatesIsNull_ShouldReturnValidationError()
    {
        var command = new UpdateLearningContentCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            Updates = null!
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Updates)
            .WithErrorMessage("Updates are required.");
    }
    #endregion

    #region JsonPatchDocument Validations
    [Fact]
    public void Validate_WhenOperationActionIsNotProvided_ShouldReturnValidationError()
    {

        UpdateLearningContentCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            Updates = new JsonPatchDocument()
            {
                Operations =
                {
                    new Operation<LearningContentDto>
                    {
                        op = string.Empty,
                        path = "/sections/-"
                    }
                }
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Updates.Operations[0].op")
            .WithErrorMessage("Valid operation type: 'add', 'replace', 'move', or 'remove' is required.");
    }

    [Fact]
    public void Validate_WhenPathIsNotProvided_ShouldReturnValidationError()
    {
        UpdateLearningContentCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            Updates = new JsonPatchDocument()
            {
                Operations =
                {
                    new Operation<LearningContentDto>
                    {
                        op = "add",
                        path = string.Empty
                    }
                }
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Updates.Operations[0].path")
            .WithErrorMessage("A valid path is required to update target location.");
    }

    [Fact]
    public void Validate_WhenPerformingMoveOperation_FromPathIsNotProvided_ShouldReturnValidationError()
    {

        UpdateLearningContentCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            Updates = new JsonPatchDocument()
            {
                Operations =
                {
                    new Operation<LearningContentDto>
                    {
                        op = "move",
                        path = "sections/0",
                        from = string.Empty
                    }
                }
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Updates.Operations[0].from")
            .WithErrorMessage("From parameter can not be null or empty.");
    }

    [Theory]
    [InlineData(AddOperation)]
    [InlineData(ReplaceOperation)]
    public void Validate_WhenPerformingAddOrReplaceOperations_ValueIsNotProvided_ShouldReturnValidationError(string operation)
    {

        UpdateLearningContentCommand.Contract command = new()
        {
            CourseId = Guid.NewGuid(),
            Updates = new JsonPatchDocument()
            {
                Operations =
                {
                    new Operation<LearningContentDto>
                    {
                        op = operation,
                        path = "sections/-",
                        value = null
                    }
                }
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Updates.Operations[0].value")
            .WithErrorMessage("Value is required.");
    }
    #endregion

    #region LearningContentSectionDto Validations

    [Fact]
    public void ValidateDto_WhenSectionNameIsNull_ShouldReturnValidationError()
    {
        LearningContentDto updatedContentDto = new()
        {
            CourseId = Guid.NewGuid(),
            Sections = new List<LearningContentSectionDto>
            {
                new LearningContentSectionDto
                {
                    SectionId = ValidSectionId, Name = null!, LearningObjects = new List<LearningObjectDto>()
                }
            }
        };

        var result = _learningContentDtoValidator.TestValidate(updatedContentDto);

        result.ShouldHaveValidationErrorFor("Sections[0].Name")
            .WithErrorMessage($"Name is required for section update.");
    }

    [Fact]
    public void ValidateDto_WhenSectionNameIsEmpty_ShouldReturnValidationError()
    {
        LearningContentDto updatedContentDto = new()
        {
            CourseId = Guid.NewGuid(),
            Sections = new List<LearningContentSectionDto>
           {
               new LearningContentSectionDto
               {
                   SectionId = ValidSectionId,
                   Name = new LocalizedStringDto
                   {
                       En = string.Empty
                   },
                   LearningObjects = new List<LearningObjectDto>()
               }
           }
        };

        var result = _learningContentDtoValidator.TestValidate(updatedContentDto);

        result.ShouldHaveValidationErrorFor("Sections.Name.En")
            .WithErrorMessage($"Name.En cannot be empty - Section Id: {ValidSectionId}");
    }

    [Fact]
    public void ValidateDto_WhenSectionIdIsEmpty_ShouldReturnValidationError()
    {
        LearningContentDto updatedContentDto = new()
        {
            CourseId = Guid.NewGuid(),
            Sections = new List<LearningContentSectionDto>
           {
               new LearningContentSectionDto
               {
                   SectionId = Guid.Empty,
                   Name = new LocalizedStringDto
                   {
                       En = ValidSectionName
                   },
                   LearningObjects = new List<LearningObjectDto>()
               }
           }
        };

        var result = _learningContentDtoValidator.TestValidate(updatedContentDto);

        result.ShouldHaveValidationErrorFor("Sections[0].SectionId")
            .WithErrorMessage("SectionId is required.");
    }
    #endregion

    #region LearningObjectDto Validations
    [Fact]
    public void Validate_WhenLearningObjectTypeIsInvalid_ShouldReturnValidationError()
    {
        LearningContentDto updatedContentDto = new()
        {
            CourseId = Guid.NewGuid(),
            Sections = new List<LearningContentSectionDto>
           {
               new LearningContentSectionDto
               {
                   SectionId = ValidSectionId,
                   Name = new LocalizedStringDto
                   {
                       En = ValidSectionName
                   },
                   LearningObjects = new List<LearningObjectDto>
                   {
                       new LearningObjectDto
                       {
                           LearningObjectId = Guid.NewGuid(),
                           LearningObjectType = (LearningObjectType)999 // Invalid type  
                       }
                   }
               }
           }
        };

        var result = _learningContentDtoValidator.TestValidate(updatedContentDto);

        result.ShouldHaveValidationErrorFor("Sections[0].LearningObjects[0].LearningObjectType")
            .WithErrorMessage("A valid Learning Object Type is required.");
    }

    // Further tests to validate Learning Object Dto will be added in future tickets
    #endregion
}