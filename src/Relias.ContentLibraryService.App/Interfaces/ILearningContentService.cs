using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;

namespace Relias.ContentLibraryService.App.Interfaces;

public interface ILearningContentService
{
    Task<LearningContentDto> InitializeByCourseIdAsync(Guid courseId, CancellationToken cancellationToken);
    Task<LearningContentDto?> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken);
    Task<LearningContentDto> UpdateLearningContentAsync(Guid courseId, JsonPatchDocument updates, LearningContentDto existingLearningContentDto, CancellationToken cancellationToken);
    Task<bool> DeleteByCourseIdAsync(Guid courseId, CancellationToken cancellationToken);
    Task<LessonDto> CreateLessonAsync(Guid courseId, Guid sectionId, CreateLessonDto lesson, LearningContentDto existingLearningContentDto, CancellationToken cancellationToken);
    Task<LessonDto> UpdateLessonAsync(Guid courseId, Guid sectionId, Guid learningObjectId, UpdateLessonDto lessonDto, LearningContentDto existingLearningContentDto, CancellationToken cancellationToken);
    Task DeleteLessonAsync(Guid courseId, Guid sectionId, Guid learningObjectId, int orgId, LearningContentDto existingLearningContentDto, CancellationToken cancellationToken);
    Task UploadLessonFileAsync(int orgId, Guid courseId, Guid learningObjectId, IFormFile chunk, string uploadId, int chunkIndex, int totalChunks, string fileName, CancellationToken cancellationToken);
}
