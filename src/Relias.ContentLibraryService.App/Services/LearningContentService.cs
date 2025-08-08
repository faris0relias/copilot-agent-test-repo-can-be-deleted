using AutoMapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Validators;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Common.Constants;
using Relias.ContentLibraryService.Common.Helpers;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Relias.ContentLibraryService.Domain.Lesson;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static System.Collections.Specialized.BitVector32;
using ValidationException = Relias.ContentLibraryService.Common.Exceptions.ValidationException;
using ValidationFailure = FluentValidation.Results.ValidationFailure;


namespace Relias.ContentLibraryService.App.Services;

public class LearningContentService(ILearningContentRepository repository, IMainBlobStorageRepository mainBlobStorageRepository, IMapper mapper, ILogger<LearningContentService> logger) : ILearningContentService
{
    public async Task<LearningContentDto> InitializeByCourseIdAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByCourseIdAsync(courseId, cancellationToken);

        if (existing is not null)
            return mapper.Map<LearningContentDto>(existing);

        var learningContent = new LearningContent
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Sections = [
                new LearningContentSection
                {
                    SectionId = Guid.NewGuid(),
                    Name = new LocalizedString { En = "Section 1" },
                    LearningObjects = []
                }
            ]
        };

        var created = await repository.CreateAsync(learningContent, cancellationToken);

        return mapper.Map<LearningContentDto>(created);
    }

    public async Task<LearningContentDto?> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var learningContent = await repository.GetByCourseIdAsync(courseId, cancellationToken);

        return mapper.Map<LearningContentDto>(learningContent);
    }

    public async Task<LearningContentDto> UpdateLearningContentAsync(Guid courseId, JsonPatchDocument updates, LearningContentDto existingLearningContentDto, CancellationToken cancellationToken)
    {
        try
        {
            updates.ApplyTo(existingLearningContentDto);

            // run validations on before saving to database
            LearningContentDtoValidator validator = new();
            ValidationResult validationResults = validator.Validate(existingLearningContentDto);

            if (!validationResults.IsValid)
            {
                List<ValidationFailure> failures = validationResults.Errors.Where(f => f != null).ToList();
                throw new ValidationException(failures);
            }
        }
        catch (JsonPatchException ex)
        {
            throw new JsonPatchException($"Invalid Request: {ex.Message}", ex);
        }

        LearningContent patchContent = mapper.Map<LearningContent>(existingLearningContentDto); //ready to send to db by mapping back to learning content entity

        LearningContent updated = await repository.UpdateAsync(patchContent, cancellationToken);
        return mapper.Map<LearningContentDto>(updated);
    }

    public async Task<LessonDto> CreateLessonAsync(Guid courseId, Guid sectionId, CreateLessonDto dto, LearningContentDto existingLearningContentDto, CancellationToken cancellationToken)
    {
        var sectionIndex = existingLearningContentDto.Sections.FindIndex(s => s.SectionId == sectionId);
        if (sectionIndex == -1)
            throw new InvalidOperationException($"Section {sectionId} not found for course {courseId}.");

        var lessonId = Guid.NewGuid();

        var lesson = new LessonDto
        {
            LearningObjectId = lessonId,
            LearningObjectType = LearningObjectType.Lesson,
            Name = dto.Name,
            DurationMinutes = dto.DurationMinutes,
            LessonType = dto.LessonType,
            RequiredForCompletion = dto.RequiredForCompletion,
            RequiresAudio = dto.RequiresAudio,
            RequiresVideo = dto.RequiresVideo,
            OpensInNewTab = dto.OpensInNewTab,
            ContentPath = dto.ContentPath,
            FileName = dto.FileName,
            FileSize = dto.FileSize,
            FormatType = dto.FormatType
        };

        var patch = new JsonPatchDocument();
        patch.Add($"/sections/{sectionIndex}/learningObjects/-", lesson);

        await UpdateLearningContentAsync(courseId, patch, existingLearningContentDto, cancellationToken);

        return lesson;
    }

    public async Task<LessonDto> UpdateLessonAsync(Guid courseId, Guid sectionId, Guid learningObjectId, UpdateLessonDto lessonDto, LearningContentDto existingLearningContentDto, CancellationToken cancellationToken)
    {
        var sectionIndex = existingLearningContentDto.Sections.FindIndex(s => s.SectionId == sectionId);
        if (sectionIndex == -1)
            throw new InvalidOperationException($"Section {sectionId} not found for course {courseId}.");

        var lessonIndex = existingLearningContentDto.Sections[sectionIndex].LearningObjects.FindIndex(l => l.LearningObjectId == learningObjectId);

        if (lessonIndex == -1)
            throw new InvalidOperationException($"Lesson {learningObjectId} not found in section {sectionId} for course {courseId}.");

        var newLesson = new LessonDto
        {
            LearningObjectId = learningObjectId,
            LearningObjectType = LearningObjectType.Lesson,
            Name = lessonDto.Name,
            DurationMinutes = lessonDto.DurationMinutes,
            LessonType = lessonDto.LessonType,
            RequiredForCompletion = lessonDto.RequiredForCompletion,
            RequiresAudio = lessonDto.RequiresAudio,
            RequiresVideo = lessonDto.RequiresVideo,
            OpensInNewTab = lessonDto.OpensInNewTab,
            ContentPath = lessonDto.ContentPath,
            FileName = lessonDto.FileName,
            FileSize = lessonDto.FileSize,
            FormatType = lessonDto.FormatType
        };
     
        if ((lessonDto.DeleteFile) && lessonDto.LessonType.Equals("file", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var existingLesson = existingLearningContentDto.Sections[sectionIndex].LearningObjects.OfType<LessonDto>().ToList()[lessonIndex];

                // Extract container name from existing lesson's ContentPath for validation
                var containerName = existingLesson.ContentPath;
                if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(lessonDto.OrgId))
                {
                    throw new InvalidOperationException($"Unable to delete file; ContentPath or OrgId is invalid. LearningObjectId: {existingLesson.LearningObjectId}.");
                }

                if (!await DeleteLessonBlobFileAsync(existingLesson, courseId.ToString(), learningObjectId.ToString(), lessonDto.OrgId, containerName))
                {
                    logger.LogWarning($"Could not delete associated file for lesson {learningObjectId}.");
                }                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while attempting to delete file for lesson {LearningObjectId}.",
                    learningObjectId);
                throw;
            }
        }

        var patch = new JsonPatchDocument();
        patch.Replace($"/sections/{sectionIndex}/learningObjects/{lessonIndex}", newLesson);
        
        await UpdateLearningContentAsync(courseId, patch, existingLearningContentDto, cancellationToken);
        logger.LogInformation("Updated lesson {LessonId} in section {SectionId} for course {CourseId}.", learningObjectId, sectionId, courseId);

        return newLesson;
    }

    public async Task DeleteLessonAsync(Guid courseId, Guid sectionId, Guid learningObjectId, int orgId, LearningContentDto existingLearningContentDto, CancellationToken cancellationToken)
    {
        var sectionIndex = existingLearningContentDto.Sections.FindIndex(s => s.SectionId == sectionId);
        if (sectionIndex == -1)
            throw new InvalidOperationException($"Section {sectionId} not found for course {courseId}.");

        var lessonIndex = existingLearningContentDto.Sections[sectionIndex].LearningObjects.FindIndex(l => l.LearningObjectId == learningObjectId);

        if (lessonIndex == -1)
            throw new InvalidOperationException($"Lesson {learningObjectId} not found in section {sectionId} for course {courseId}.");

        var existingLesson = existingLearningContentDto.Sections[sectionIndex].LearningObjects.OfType<LessonDto>().ElementAt(lessonIndex);

        // Delete associated blob file if it exists
        if (!string.IsNullOrEmpty(existingLesson.FileName) && !string.IsNullOrEmpty(existingLesson.ContentPath) && existingLesson.LessonType.Equals("file", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await DeleteLessonBlobFileAsync(existingLesson, courseId.ToString(), learningObjectId.ToString(), orgId.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while attempting to delete file for lesson {LearningObjectId}.", learningObjectId);
            }
        }

        var patch = new JsonPatchDocument();
        patch.Remove($"/sections/{sectionIndex}/learningObjects/{lessonIndex}");

        await UpdateLearningContentAsync(courseId, patch, existingLearningContentDto, cancellationToken);
        logger.LogInformation("Deleted lesson {LearningObjectId} from section {SectionId} for course {CourseId}.", learningObjectId, sectionId, courseId);
    }

    public async Task UploadLessonFileAsync(int orgId, Guid courseId, Guid learningObjectId, IFormFile chunk, string uploadId, int chunkIndex, int totalChunks, string fileName, CancellationToken cancellationToken)
    {
        var containerName = AzureSdkClientConstants.MainContainer;
        var path = $"{orgId}/{courseId}/{learningObjectId}";
        string blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{chunkIndex:D6}"));

        try
        {
            logger.LogInformation("Starting upload of chunk {ChunkIndex}/{TotalChunks} for file {FileName} (uploadId: {UploadId})", chunkIndex, totalChunks, fileName, uploadId);

            // Use CopyToAsync with a memory stream to ensure the chunk stays in memory
            using var memoryStream = new MemoryStream();
            await chunk.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0; // Reset position for reading
            
            await mainBlobStorageRepository.StageBlockAsync(containerName, path, fileName, blockId, memoryStream);

            logger.LogDebug("Successfully staged block {BlockId} for uploadId {UploadId}", blockId, uploadId);

            bool isComplete = BlockIdMemoryStore.Add(uploadId, chunkIndex, blockId, totalChunks);
            if (isComplete)
            {
                logger.LogInformation("All chunks received for uploadId {UploadId}. Committing block list for file {FileName}", uploadId, fileName);
                try
                {
                    var blockIds = BlockIdMemoryStore.Get(uploadId);
                    await mainBlobStorageRepository.CommitBlockListAsync(containerName, path, fileName, blockIds);
                    logger.LogInformation("Successfully committed block list for file {FileName} (uploadId: {UploadId})", fileName, uploadId);
                }
                finally
                {
                    BlockIdMemoryStore.Remove(uploadId);
                    logger.LogDebug("Removed uploadId {UploadId} from memory store", uploadId);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading chunk {ChunkIndex}/{TotalChunks} for file {FileName} (uploadId: {UploadId})", chunkIndex, totalChunks, fileName, uploadId);
            BlockIdMemoryStore.Remove(uploadId);
            throw;
        }
    }

    public async Task<bool> DeleteByCourseIdAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByCourseIdAsync(courseId, cancellationToken);
        if (existing is null)
        {
            return false;
        }
        await repository.DeleteAsync(existing, cancellationToken);

        return true;
    }

    private async Task<bool> DeleteLessonBlobFileAsync(LessonDto existingLesson, string courseId, string learningObjectId, string orgId, string? containerName = null)
    {
        if (string.IsNullOrEmpty(existingLesson.FileName))
        {
            logger.LogWarning("Unable to delete file; existingLesson.FileName is invalid. LearningObjectId: {LearningObjectId}.", existingLesson.LearningObjectId);
            return false;
        }

        var targetContainer = containerName ?? AzureSdkClientConstants.MainContainer;
        var contentPath = $"{orgId}/{courseId}/{learningObjectId}/";

        var deleted = await mainBlobStorageRepository.RemoveBlobAsync(
            targetContainer, contentPath, existingLesson.FileName).ConfigureAwait(false);

        if (deleted)
        {
            logger.LogInformation("Deleted file {FileName} from container {ContainerName} with contentPath {ContentPath}.", existingLesson.FileName, targetContainer, contentPath);
        }
        else
        {
            logger.LogWarning("Failed to delete file {FileName} from container {ContainerName} with contentPath {ContentPath}.", existingLesson.FileName, targetContainer, contentPath);
        }

        return deleted;
    }
}
