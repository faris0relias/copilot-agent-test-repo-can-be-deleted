using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Interfaces;

public interface IFinalExamRepository
{
    Task<FinalExam> CreateFinalExamAsync(Guid courseId, string? userId, CancellationToken cancellationToken);
    Task<FinalExam?> GetFinalExamByCourseIdAsync(Guid courseId, CancellationToken cancellationToken);
    Task<FinalExam?> GetFinalExamSettingsAsync(Guid courseId, CancellationToken cancellationToken);

    Task<FinalExam?> GetLearnerFinalExamByCourseIdAsync(Guid courseId, List<Guid>? questionIds, bool isCompleted, CancellationToken cancellationToken);

    Task<FinalExam> UpdateFinalExamAsync(Guid courseId, string? userId, Dictionary<string, dynamic> updatedValues, CancellationToken cancellationToken);
    Task DeleteFinalExamAsync(FinalExam finalExam, CancellationToken cancellationToken);
    Task<FinalExam> UpdateFinalExamQuestionAsync(FinalExam updated, string? userId, CancellationToken cancellationToken);

    Task<FinalExam?> GetLearnerFinalExamQuestionAnswerIdsAsync(Guid courseId, CancellationToken cancellationToken);
    
}