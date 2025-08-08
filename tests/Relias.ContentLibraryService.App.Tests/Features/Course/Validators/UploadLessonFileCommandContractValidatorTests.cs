using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Validators;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Validators;

public class UploadLessonFileCommandContractValidatorTests
{
    private readonly UploadLessonFileCommandContractValidator _validator = new();

    public record TestUploadContract
    {
        public Guid CourseId { get; init; }
        public Guid LearningObjectId { get; init; }
        public int OrganizationId { get; init; }
        public IFormFile Chunk { get; init; } = null!;
        public string UploadId { get; init; } = string.Empty;
        public int ChunkIndex { get; init; }
        public int TotalChunks { get; init; }
        public string FileName { get; init; } = string.Empty;
    }

    private static TestUploadContract ValidTestContract => new()
    {
        CourseId = Guid.NewGuid(),
        LearningObjectId = Guid.NewGuid(),
        OrganizationId = 1,
        Chunk = CreateMockFormFile(),
        UploadId = "upload-123",
        ChunkIndex = 0,
        TotalChunks = 3,
        FileName = "lesson.mp4"
    };

    private static IFormFile CreateMockFormFile(long length = 5)
    {
        var mockFile = new Mock<IFormFile>();
        var content = new byte[length];
        var stream = new MemoryStream(content);

        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.FileName).Returns("test.mp4");
        mockFile.Setup(f => f.ContentType).Returns("video/mp4");

        return mockFile.Object;
    }

    private static UploadLessonFileCommand.Contract CreateContract(TestUploadContract testContract)
    {
        return new UploadLessonFileCommand.Contract
        {
            CourseId = testContract.CourseId,
            LearningObjectId = testContract.LearningObjectId,
            OrganizationId = testContract.OrganizationId,
            Chunk = testContract.Chunk,
            UploadId = testContract.UploadId,
            ChunkIndex = testContract.ChunkIndex,
            TotalChunks = testContract.TotalChunks,
            FileName = testContract.FileName
        };
    }

    [Fact]
    public void Validate_WhenCourseIdIsEmpty_ShouldReturnValidationError()
    {
        var testContract = ValidTestContract with { CourseId = Guid.Empty };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.CourseId)
              .WithErrorMessage("Course Id cannot be an empty GUID.");
    }

    [Fact]
    public void Validate_WhenLearningObjectIdIsEmpty_ShouldReturnValidationError()
    {
        var testContract = ValidTestContract with { LearningObjectId = Guid.Empty };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.LearningObjectId)
              .WithErrorMessage("Learning Object Id cannot be an empty GUID.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WhenOrganizationIdIsNotGreaterThanZero_ShouldReturnValidationError(int organizationId)
    {
        var testContract = ValidTestContract with { OrganizationId = organizationId };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.OrganizationId)
              .WithErrorMessage("Organization Id must be greater than 0.");
    }

    [Fact]
    public void Validate_WhenChunkIsNull_ShouldReturnValidationError()
    {
        var testContract = ValidTestContract with { Chunk = null! };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.Chunk)
              .WithErrorMessage("File chunk is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenUploadIdIsInvalid_ShouldReturnValidationError(string invalidUploadId)
    {
        var testContract = ValidTestContract with { UploadId = invalidUploadId };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.UploadId)
              .WithErrorMessage("Upload Id is required.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WhenChunkIndexIsNegative_ShouldReturnValidationError(int chunkIndex)
    {
        var testContract = ValidTestContract with { ChunkIndex = chunkIndex };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.ChunkIndex)
              .WithErrorMessage("Chunk Index must be greater than or equal to 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WhenTotalChunksIsNotGreaterThanZero_ShouldReturnValidationError(int totalChunks)
    {
        var testContract = ValidTestContract with { TotalChunks = totalChunks };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.TotalChunks)
              .WithErrorMessage("Total Chunks must be greater than 0.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenFileNameIsInvalid_ShouldReturnValidationError(string invalidFileName)
    {
        var testContract = ValidTestContract with { FileName = invalidFileName };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.FileName)
              .WithErrorMessage("File Name is required.");
    }

    [Theory]
    [InlineData("lesson.txt")]
    [InlineData("lesson.doc")]
    [InlineData("lesson.exe")]
    [InlineData("lesson")]
    [InlineData("lesson.avi")]
    public void Validate_WhenFileNameHasInvalidExtension_ShouldReturnValidationError(string fileName)
    {
        var testContract = ValidTestContract with { FileName = fileName };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.FileName)
              .WithErrorMessage("File must have a valid extension (.mp3, .mp4, .pdf, or .zip).");
    }

    [Theory]
    [InlineData("lesson.mp3")]
    [InlineData("lesson.mp4")]
    [InlineData("lesson.pdf")]
    [InlineData("lesson.zip")]
    [InlineData("lesson.MP3")]
    [InlineData("lesson.MP4")]
    [InlineData("lesson.PDF")]
    [InlineData("lesson.ZIP")]
    public void Validate_WhenFileNameHasValidExtension_ShouldNotReturnValidationError(string fileName)
    {
        var testContract = ValidTestContract with { FileName = fileName };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldNotHaveValidationErrorFor(c => c.FileName);
    }

    [Theory]
    [InlineData("file<name.mp4")]
    [InlineData("file|name.mp4")]
    [InlineData("file\tname.mp4")]
    public void Validate_WhenFileNameContainsRestrictedCharacters_ShouldReturnValidationError(string fileName)
    {
        var testContract = ValidTestContract with { FileName = fileName };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.FileName)
              .WithErrorMessage("File name contains invalid characters.");
    }

    [Theory]
    [InlineData("CON.mp4")]
    [InlineData("COM1.mp4")]
    [InlineData("con.pdf")]
    public void Validate_WhenFileNameIsReservedName_ShouldReturnValidationError(string fileName)
    {
        var testContract = ValidTestContract with { FileName = fileName };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.FileName)
              .WithErrorMessage("File name contains restricted system names.");
    }

    [Fact]
    public void Validate_WhenFileNameIsTooLong_ShouldReturnValidationError()
    {
        var longFileName = new string('a', 252) + ".mp4"; // 256 characters total
        var testContract = ValidTestContract with { FileName = longFileName };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c.FileName)
              .WithErrorMessage("File name must be between 1 and 255 characters.");
    }

    [Fact]
    public void Validate_WhenChunkIndexIsEqualToTotalChunks_ShouldReturnValidationError()
    {
        var testContract = ValidTestContract with { ChunkIndex = 3, TotalChunks = 3 };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c)
              .WithErrorMessage("Chunk Index must be less than Total Chunks.");
    }

    [Fact]
    public void Validate_WhenChunkIndexIsGreaterThanTotalChunks_ShouldReturnValidationError()
    {
        var testContract = ValidTestContract with { ChunkIndex = 5, TotalChunks = 3 };
        var contract = CreateContract(testContract);

        var result = _validator.TestValidate(contract);
        result.ShouldHaveValidationErrorFor(c => c)
              .WithErrorMessage("Chunk Index must be less than Total Chunks.");
    }

    [Fact]
    public void Validate_WhenMultipleFieldsAreInvalid_ShouldReturnAllValidationErrors()
    {
        var contract = new UploadLessonFileCommand.Contract
        {
            CourseId = Guid.Empty,
            LearningObjectId = Guid.Empty,
            OrganizationId = 0,
            Chunk = null!,
            UploadId = "",
            ChunkIndex = -1,
            TotalChunks = 0,
            FileName = "lesson.txt"
        };

        var result = _validator.TestValidate(contract);

        result.ShouldHaveValidationErrorFor(c => c.CourseId);
        result.ShouldHaveValidationErrorFor(c => c.LearningObjectId);
        result.ShouldHaveValidationErrorFor(c => c.OrganizationId);
        result.ShouldHaveValidationErrorFor(c => c.Chunk);
        result.ShouldHaveValidationErrorFor(c => c.UploadId);
        result.ShouldHaveValidationErrorFor(c => c.ChunkIndex);
        result.ShouldHaveValidationErrorFor(c => c.TotalChunks);
        result.ShouldHaveValidationErrorFor(c => c.FileName);
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotReturnAnyValidationErrors()
    {
        var contract = CreateContract(ValidTestContract);

        var result = _validator.TestValidate(contract);
        result.ShouldNotHaveAnyValidationErrors();
    }
}