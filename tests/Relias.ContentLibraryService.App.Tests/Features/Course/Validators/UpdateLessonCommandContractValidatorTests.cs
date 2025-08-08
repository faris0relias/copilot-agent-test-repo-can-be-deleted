using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Validators;
using Relias.ContentLibraryService.Domain.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class UpdateLessonCommandContractValidatorTests
{
    private readonly UpdateLessonCommandContractValidator _validator = new();

    private const string NewLessonName = "New Lesson name";
    private const string NewLessonType = "file";

    private static UpdateLessonDto ValidLessonDto => new()
    {
        LearningObjectType = LearningObjectType.Lesson,
        LearningObjectId = Guid.NewGuid(),
        Name = new LocalizedStringDto { En = NewLessonName },
        LessonType = NewLessonType,
        DurationMinutes = 10,
        RequiredForCompletion = true,
        RequiresAudio = false,
        RequiresVideo = false,
        OpensInNewTab = false,
        ContentPath = "/path",
        FormatType = LessonFormatType.Pdf
    };

    [Fact]
    public void Validate_WhenCourseIdIsEmpty_ShouldReturnValidationError()
    {
        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.Empty,
            SectionId = Guid.NewGuid(),
            LearningObjectId = Guid.NewGuid(),
            LessonDto = ValidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CourseId)
              .WithErrorMessage("CourseId is required.");
    }

    [Fact]
    public void Validate_WhenSectionIdIsEmpty_ShouldReturnValidationError()
    {
        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.Empty,
            LearningObjectId = Guid.NewGuid(),
            LessonDto = ValidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.SectionId)
              .WithErrorMessage("SectionId is required.");
    }

    [Fact]
    public void Validate_WhenLearningObjectIdIsEmpty_ShouldReturnValidationError()
    {
        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = Guid.Empty,
            LessonDto = ValidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.LearningObjectId)
              .WithErrorMessage("LearningObjectId is required.");
    }

    [Fact]
    public void Validate_WhenUpdateLessonDtoIsNull_ShouldReturnValidationError()
    {
        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = Guid.NewGuid(),
            LessonDto = null!
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.LessonDto)
              .WithErrorMessage("LessonDto is required.");
    }

    [Fact]
    public void Validate_WhenDurationLessThanZero_ShouldReturnValidationError()
    {
        var invalidLessonDto = new UpdateLessonDto
        {
            LearningObjectType = LearningObjectType.Lesson,
            LearningObjectId = Guid.NewGuid(),
            Name = new LocalizedStringDto { En = NewLessonName },
            LessonType = NewLessonType,
            DurationMinutes = -1,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = "/path",
            FormatType = LessonFormatType.Pdf
        };

        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.DurationMinutes")
              .WithErrorMessage("DurationMinutes must be greater than or equal to 0.");
    }

    [Fact]
    public void Validate_WhenDurationIsZero_ShouldNotReturnAnyValidationErrors()
    {
        var zeroDurationLessonDto = new UpdateLessonDto
        {
            LearningObjectType = LearningObjectType.Lesson,
            LearningObjectId = Guid.NewGuid(),
            Name = new LocalizedStringDto { En = NewLessonName },
            LessonType = NewLessonType,
            DurationMinutes = 0,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = "/path",
            FormatType = LessonFormatType.Pdf
        };

        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = Guid.NewGuid(),
            LessonDto = zeroDurationLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenLessonTypeIsInvalid_ShouldReturnValidationError(string invalidLessonType)
    {
        var invalidLessonDto = new UpdateLessonDto
        {
            LearningObjectType = LearningObjectType.Lesson,
            LearningObjectId = Guid.NewGuid(),
            Name = new LocalizedStringDto { En = NewLessonName },
            LessonType = invalidLessonType,
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = "/path",
            FormatType = LessonFormatType.Pdf
        };

        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.LessonType")
              .WithErrorMessage("Must be a valid lesson type.");
    }

    [Fact]
    public void Validate_WhenLessonTypeIsUrl_AndOpensInNewTabIsNull_ShouldReturnValidationError()
    {
        var invalidLessonDto = new UpdateLessonDto
        {
            LearningObjectType = LearningObjectType.Lesson,
            LearningObjectId = Guid.NewGuid(),
            Name = new LocalizedStringDto { En = NewLessonName },
            LessonType = "url",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            ContentPath = "/path",
            FormatType = LessonFormatType.Url
        };

        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.OpensInNewTab")
              .WithErrorMessage("OpensInNewTab is required when LessonType is 'url'.");
    }

    [Theory]
    [InlineData("http://www.example.com")]
    [InlineData("https://www exam ple com")]
    [InlineData(null)]
    public void Validate_WhenLessonTypeIsUrl_AndContentPathIsNotAValidHttpsUrl_ShouldReturnValidationError(string? invalidUrl)
    {
        var invalidLessonDto = new UpdateLessonDto
        {
            LearningObjectType = LearningObjectType.Lesson,
            LearningObjectId = Guid.NewGuid(),
            Name = new LocalizedStringDto { En = NewLessonName },
            LessonType = "url",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = true,
            ContentPath = invalidUrl,
            FormatType = LessonFormatType.Url
        };

        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.ContentPath")
              .WithErrorMessage("ContentPath must be a valid HTTPS URL.");
    }

    [Fact]
    public void Validate_WhenMultipleFieldsAreInvalid_ShouldReturnAllValidationErrors()
    {
        var invalidLessonDto = new UpdateLessonDto
        {
            LearningObjectType = LearningObjectType.Lesson,
            LearningObjectId = Guid.NewGuid(),
            Name = null!, // Invalid
            LessonType = null!, // Invalid
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            FormatType = LessonFormatType.Pdf
        };

        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("LessonDto.LessonType")
              .WithErrorMessage("Must be a valid lesson type.");

        result.ShouldHaveValidationErrorFor("LessonDto.Name")
              .WithErrorMessage("Name is required.");

        result.ShouldHaveValidationErrorFor("LessonDto.ContentPath")
              .WithErrorMessage("ContentPath is required.");
    }
    
    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotReturnAnyValidationErrors()
    {
        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = Guid.NewGuid(),
            LessonDto = ValidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenLessonTypeIsUrl_AndFormatTypeIsNotUrl_ShouldReturnValidationError()
    {
        var dto = new UpdateLessonDto
        {
            LearningObjectType = LearningObjectType.Lesson,
            LearningObjectId = Guid.NewGuid(),
            Name = new LocalizedStringDto { En = "Url Lesson" },
            LessonType = "url",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = true,
            ContentPath = "https://valid.url",
            FormatType = LessonFormatType.Pdf
        };

        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = dto.LearningObjectId,
            LessonDto = dto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.FormatType")
              .WithErrorMessage("FormatType must be 'Url' when LessonType is 'url'.");
    }

    [Fact]
    public void Validate_WhenLessonTypeIsFile_AndFormatTypeIsInvalid_ShouldReturnValidationError()
    {
        var dto = new UpdateLessonDto
        {
            LearningObjectType = LearningObjectType.Lesson,
            LearningObjectId = Guid.NewGuid(),
            Name = new LocalizedStringDto { En = "File Lesson" },
            LessonType = "file",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = "/main/8/test.pdf",
            FormatType = LessonFormatType.Url
        };

        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = dto.LearningObjectId,
            LessonDto = dto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.FormatType")
              .WithErrorMessage("FormatType must be one of: Pdf, Audio, Video, Scorm, Aicc when LessonType is 'file'.");
    }

    [Theory]
    [InlineData(LessonFormatType.Pdf)]
    [InlineData(LessonFormatType.Audio)]
    [InlineData(LessonFormatType.Video)]
    [InlineData(LessonFormatType.Scorm)]
    [InlineData(LessonFormatType.Aicc)]
    public void Validate_WhenLessonTypeIsFile_AndFormatTypeIsValid_ShouldNotReturnValidationError(LessonFormatType validFormat)
    {
        var dto = new UpdateLessonDto
        {
            LearningObjectType = LearningObjectType.Lesson,
            LearningObjectId = Guid.NewGuid(),
            Name = new LocalizedStringDto { En = "File Lesson" },
            LessonType = "file",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = "/main/8/test.pdf",
            FormatType = validFormat
        };

        var command = new UpdateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LearningObjectId = dto.LearningObjectId,
            LessonDto = dto
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor("LessonDto.FormatType");
    }
}
