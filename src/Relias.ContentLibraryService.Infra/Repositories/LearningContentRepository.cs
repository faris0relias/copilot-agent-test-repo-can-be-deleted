using Microsoft.Azure.Cosmos;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Relias.ContentLibraryService.Infra.Cosmos;

namespace Relias.ContentLibraryService.Infra.Repositories;

public class LearningContentRepository(ICosmosClientWrapper clientWrapper) : ILearningContentRepository
{
    public async Task<LearningContent?> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var results = await clientWrapper.QueryFirstOrDefaultAsync<LearningContent>(
            q => q.Where(lc => lc.CourseId == courseId),
            cancellationToken);

        return results;
    }

    public async Task<LearningContent> CreateAsync(LearningContent learningContent, CancellationToken cancellationToken)
    {
        var createdEntity = await clientWrapper.CreateItemAsync(learningContent, cancellationToken);

        return createdEntity;
    }

    public async Task<LearningContent> UpdateAsync(LearningContent updated, CancellationToken cancellationToken)
    {
        string itemId = updated.Id.ToString();
        var updatedEntity = await clientWrapper.UpdateItemAsync(updated, itemId, cancellationToken);

        return updatedEntity;
    }

    public async Task DeleteAsync (LearningContent learningContent, CancellationToken cancellationToken)
    {
        if (learningContent == null)
        {
            throw new ArgumentNullException(nameof(learningContent), "Learning content cannot be null");
        }
        var partitionKey = new PartitionKey(learningContent.CourseId.ToString());
        await clientWrapper.DeleteItemAsync<LearningContent>(learningContent.Id.ToString(), partitionKey,cancellationToken);
    }
}
