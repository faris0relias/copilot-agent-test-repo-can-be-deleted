using Microsoft.Azure.Cosmos;
using Relias.ContentLibraryService.App.Helpers;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Domain.Course.FinalExam;
using Relias.ContentLibraryService.Infra.Cosmos;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Relias.ContentLibraryService.Infra.Repositories;

public class FinalExamRepository(ICosmosClientWrapper cosmosClientWrapper)
    : IFinalExamRepository
{
    public async Task<FinalExam?> GetFinalExamByCourseIdAsync(Guid courseId, CancellationToken cancellationToken)
    {
        return await cosmosClientWrapper.QueryFirstOrDefaultAsync<FinalExam>(
        q => q.Where(finalExam => finalExam.CourseId == courseId), cancellationToken);
    }

    
    public async Task<FinalExam?> GetFinalExamSettingsAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var finalExam = await cosmosClientWrapper.QueryFirstOrDefaultAsync<FinalExam>(
            q => q.Where(finalExam => finalExam.CourseId == courseId && finalExam.QuestionsDisplayedPerExam >= 2), cancellationToken);

        if (finalExam != null)
        {
            // Create a new instance of FinalExam with the required properties
            finalExam = new FinalExam
            {
                CourseId = finalExam.CourseId,
                MinimumPercentageToPass = finalExam.MinimumPercentageToPass,
                Duration = finalExam.Duration,
                QuestionsDisplayedPerExam = finalExam.QuestionsDisplayedPerExam,
                Created = finalExam.Created
            };
        }
        return finalExam;
    }
    

    public async Task<FinalExam?> GetLearnerFinalExamByCourseIdAsync(Guid courseId, List<Guid>? questionIds, bool isCompleted, CancellationToken cancellationToken)
    {
        var finalExam = await cosmosClientWrapper.QueryFirstOrDefaultAsync<FinalExam>(
        q => q.Where(e =>
        e.CourseId == courseId &&
        e.FinalExamQuestions != null &&
        questionIds != null &&
        e.FinalExamQuestions.Any(q => questionIds.Contains(q.QuestionId))
    ),
    cancellationToken);

        if (finalExam != null && questionIds != null)
        {
            // Create a dictionary for lookups
            var questionDict = finalExam.FinalExamQuestions!.ToDictionary(q => q.QuestionId);

            // Only keep questions that match the input IDs, in the same order
            finalExam.FinalExamQuestions = questionIds
                .Where(questionDict.ContainsKey)
                .Select(id => questionDict[id])
                .ToList();

        }

        return finalExam;
    }

    public async Task<FinalExam?> GetLearnerFinalExamQuestionAnswerIdsAsync(Guid courseId, CancellationToken cancellationToken)
    {
        return await cosmosClientWrapper.QueryFirstOrDefaultAsync<FinalExam>(
        q => q.Where(e => e.CourseId == courseId 
                       && e.FinalExamQuestions!= null 
                       && e.QuestionsDisplayedPerExam>= 2), cancellationToken);
    }

    public async Task<FinalExam> CreateFinalExamAsync(Guid courseId,  string? userId, CancellationToken cancellationToken)
    {
        FinalExam exam = new()
        {
            CourseId = courseId,
            Created = DateTime.UtcNow,
            CreatedBy = userId, 
        };

        var finalExam = await cosmosClientWrapper.CreateItemAsync(exam, cancellationToken);
        return finalExam;
    }

    public async Task<FinalExam> UpdateFinalExamAsync(Guid courseId, string? userId, Dictionary<string, dynamic> updatedValues, CancellationToken cancellationToken)
    {
        var existingFinalExam = await GetFinalExamByCourseIdAsync(courseId, cancellationToken) ?? throw new BadRequestException($"Final exam with course id {courseId} does not exist!");

        List<string> propertiesToPatch = new();
        var finalExamProperties = existingFinalExam.GetType().GetProperties();
        foreach (var pair in updatedValues)
        {
            var matchingProperty = finalExamProperties.FirstOrDefault(p => string.Equals(p.Name, pair.Key, StringComparison.OrdinalIgnoreCase));

            if (matchingProperty is not null)
            {
                if (pair.Value is null || (pair.Value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Null))
                {
                    matchingProperty.SetValue(existingFinalExam, null);
                }
                else
                {
                    var propertyValue = JsonConverterHelper.ParsePropertyValue(pair.Key.ToLower(), pair.Value);
                    matchingProperty.SetValue(existingFinalExam, propertyValue);
                }
                propertiesToPatch.Add(matchingProperty.Name);
            }
        }
        if (propertiesToPatch.Count == 0)
        {
            throw new BadRequestException($"No valid properties to update for final exam with course id {courseId}");
        }

        existingFinalExam.LastModified = DateTime.UtcNow;
        existingFinalExam.LastModifiedBy = userId;

        propertiesToPatch.Add(nameof(FinalExam.LastModified));
        propertiesToPatch.Add(nameof(FinalExam.LastModifiedBy));

        var updatedFinalExam = await cosmosClientWrapper.PatchItemAsync(existingFinalExam, existingFinalExam.Id, new PartitionKey(existingFinalExam.CourseId.ToString()), propertiesToPatch, cancellationToken);
        return updatedFinalExam;
    }

    public async Task DeleteFinalExamAsync(FinalExam finalExam, CancellationToken cancellationToken)
    {
        await cosmosClientWrapper.DeleteItemAsync<FinalExam>(finalExam.Id.ToString(), new PartitionKey(finalExam.CourseId.ToString()), cancellationToken);
    }
    public async Task<FinalExam> UpdateFinalExamQuestionAsync(FinalExam updated, string? userId, CancellationToken cancellationToken)
    {
        string itemId = updated.Id.ToString();
        updated.LastModified = DateTime.UtcNow;
        updated.LastModifiedBy = userId;
        var updatedEntity = await cosmosClientWrapper.UpdateItemAsync(updated, itemId, cancellationToken);

        return updatedEntity;
    }
}
