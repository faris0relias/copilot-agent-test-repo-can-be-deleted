using FluentValidation.TestHelper;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Validators;
using Relias.ContentLibraryService.Domain.Course.Enums;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class CreateLessonCommandContractValidatorTests
{
    private readonly CreateLessonCommandContractValidator _validator = new();

    private const string ValidLessonName = "Valid Lesson";
    private const string ValidLessonType = "file";

    private static CreateLessonDto ValidLessonDto => new()
    {
        Name = new LocalizedStringDto { En = ValidLessonName },
        LessonType = ValidLessonType,
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
        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.Empty,
            SectionId = Guid.NewGuid(),
            LessonDto = ValidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CourseId)
              .WithErrorMessage("CourseId is required.");
    }

    [Fact]
    public void Validate_WhenSectionIdIsEmpty_ShouldReturnValidationError()
    {
        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.Empty,
            LessonDto = ValidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.SectionId)
              .WithErrorMessage("SectionId is required.");
    }

    [Fact]
    public void Validate_WhenLessonDtoIsNull_ShouldReturnValidationError()
    {
        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LessonDto = null!
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.LessonDto)
              .WithErrorMessage("LessonDto is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenLessonTypeIsInvalid_ShouldReturnValidationError(string invalidLessonType)
    {
        var invalidLessonDto = new CreateLessonDto
        {
            Name = ValidLessonDto.Name,
            LessonType = invalidLessonType,
            DurationMinutes = ValidLessonDto.DurationMinutes,
            RequiredForCompletion = ValidLessonDto.RequiredForCompletion,
            RequiresAudio = ValidLessonDto.RequiresAudio,
            RequiresVideo = ValidLessonDto.RequiresVideo,
            OpensInNewTab = ValidLessonDto.OpensInNewTab,
            ContentPath = ValidLessonDto.ContentPath
        };

        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.LessonType")
              .WithErrorMessage("Must be a valid lesson type.");
    }

    [Fact]
    public void Validate_WhenLessonNameIsMissing_ShouldReturnValidationError()
    {
        var invalidLessonDto = new CreateLessonDto
        {
            Name = null!,
            LessonType = ValidLessonType,
            DurationMinutes = ValidLessonDto.DurationMinutes,
            RequiredForCompletion = ValidLessonDto.RequiredForCompletion,
            RequiresAudio = ValidLessonDto.RequiresAudio,
            RequiresVideo = ValidLessonDto.RequiresVideo,
            OpensInNewTab = ValidLessonDto.OpensInNewTab,
            ContentPath = ValidLessonDto.ContentPath,
            FormatType = LessonFormatType.Pdf
        };

        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.Name")
              .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Validate_WhenLessonNameEnIsEmpty_ShouldReturnValidationError()
    {
        var invalidLessonDto = new CreateLessonDto
        {
            Name = new LocalizedStringDto { En = "" },
            LessonType = ValidLessonType,
            DurationMinutes = ValidLessonDto.DurationMinutes,
            RequiredForCompletion = ValidLessonDto.RequiredForCompletion,
            RequiresAudio = ValidLessonDto.RequiresAudio,
            RequiresVideo = ValidLessonDto.RequiresVideo,
            OpensInNewTab = ValidLessonDto.OpensInNewTab,
            ContentPath = ValidLessonDto.ContentPath,
            FormatType = LessonFormatType.Pdf
        };

        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.Name.En")
              .WithErrorMessage("Name.En is required.");
    }

    [Fact]
    public void Validate_WhenDurationLessThanZero_ShouldReturnValidationError()
    {
        var invalidLessonDto = new CreateLessonDto
        {
            Name = new LocalizedStringDto { En = ValidLessonName },
            LessonType = ValidLessonType,
            DurationMinutes = -1,
            RequiredForCompletion = ValidLessonDto.RequiredForCompletion,
            RequiresAudio = ValidLessonDto.RequiresAudio,
            RequiresVideo = ValidLessonDto.RequiresVideo,
            OpensInNewTab = ValidLessonDto.OpensInNewTab,
            ContentPath = ValidLessonDto.ContentPath,
            FormatType = LessonFormatType.Pdf
        };

        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.DurationMinutes")
              .WithErrorMessage("DurationMinutes must be greater than or equal to 0.");
    }

    [Fact]
    public void Validate_WhenLessonTypeIsUrl_AndOpensInNewTabIsNull_ShouldReturnValidationError()
    {
        var invalidLessonDto = new CreateLessonDto
        {
            Name = new LocalizedStringDto { En = ValidLessonName },
            LessonType = "url",
            DurationMinutes = -1,
            RequiredForCompletion = ValidLessonDto.RequiredForCompletion,
            RequiresAudio = ValidLessonDto.RequiresAudio,
            RequiresVideo = ValidLessonDto.RequiresVideo,
            OpensInNewTab = null,
            ContentPath = ValidLessonDto.ContentPath,
            FormatType = LessonFormatType.Url
        };

        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
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
        var invalidLessonDto = new CreateLessonDto
        {
            Name = new LocalizedStringDto { En = ValidLessonName },
            LessonType = "url",
            DurationMinutes = -1,
            RequiredForCompletion = ValidLessonDto.RequiredForCompletion,
            RequiresAudio = ValidLessonDto.RequiresAudio,
            RequiresVideo = ValidLessonDto.RequiresVideo,
            OpensInNewTab = ValidLessonDto.OpensInNewTab,
            ContentPath = invalidUrl,
            FormatType = LessonFormatType.Url
        };

        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.ContentPath")
              .WithErrorMessage("ContentPath must be a valid HTTPS URL.");
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotReturnAnyValidationErrors()
    {
        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LessonDto = ValidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenLessonTypeIsUrl_AndFormatTypeIsNotUrl_ShouldReturnValidationError()
    {
        var invalidLessonDto = new CreateLessonDto
        {
            Name = new LocalizedStringDto { En = ValidLessonName },
            LessonType = "url",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = true,
            ContentPath = "https://example.com/resource",
            FormatType = LessonFormatType.Pdf
        };

        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("LessonDto.FormatType")
              .WithErrorMessage("FormatType must be 'Url' when LessonType is 'url'.");
    }

    [Fact]
    public void Validate_WhenLessonTypeIsFile_AndFormatTypeIsInvalid_ShouldReturnValidationError()
    {
        var invalidLessonDto = new CreateLessonDto
        {
            Name = new LocalizedStringDto { En = ValidLessonName },
            LessonType = "file",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = "/some/path/file.pdf",
            FormatType = LessonFormatType.Url
        };

        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LessonDto = invalidLessonDto
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
        var lessonDto = new CreateLessonDto
        {
            Name = new LocalizedStringDto { En = ValidLessonName },
            LessonType = "file",
            DurationMinutes = 10,
            RequiredForCompletion = true,
            RequiresAudio = false,
            RequiresVideo = false,
            OpensInNewTab = false,
            ContentPath = "/some/path",
            FormatType = validFormat
        };

        var command = new CreateLessonCommand.Contract
        {
            CourseId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            LessonDto = lessonDto
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor("LessonDto.FormatType");
    }
}
