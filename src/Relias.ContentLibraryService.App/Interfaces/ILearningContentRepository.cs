using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.App.Interfaces;

public interface ILearningContentRepository
{
    Task<LearningContent?> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken);
    Task<LearningContent> CreateAsync(LearningContent learningContent, CancellationToken cancellationToken);
    Task<LearningContent> UpdateAsync(LearningContent updated, CancellationToken cancellationToken);
    Task DeleteAsync(LearningContent learningContent, CancellationToken cancellationToken);
}
