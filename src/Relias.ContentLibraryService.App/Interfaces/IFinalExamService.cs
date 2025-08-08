using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;

namespace Relias.ContentLibraryService.App.Interfaces;

public interface IFinalExamService
{
    Task<FinalExamDto?> GetFinalExamByCourseIdAsync(Guid courseId, string lang ,CancellationToken cancellationToken);
    Task<FinalExamDto> CreateFinalExamAsync(Guid courseId, int organizationId, CancellationToken cancellationToken);
    Task<FinalExamDto> UpdateFinalExamAsync(Guid courseId, int organizationId, Dictionary<string, dynamic> finalExam, CancellationToken cancellationToken);
    Task DeleteFinalExamAsync(Guid courseId, CancellationToken cancellationToken);
    Task<FinalExamDto> UpdateFinalExamQuestionAsync(Guid courseId, Guid examId, Guid questionId, FinalExamQuestionPatchDto updates, CancellationToken cancellationToken);

    Task<Object?> GetQuestionsByIdsAsync(Guid courseId, List<Guid> questionIds, bool isCompleted, string lang ,CancellationToken cancellationToken);
    Task<FinalExamSettingWithAnswerDto?> GetLearnerFinalExamQuestionAnswerIdsAsync(Guid courseId, CancellationToken cancellationToken);
    Task<FinalExamSettingDto?> GetFinalExamSettingsAsync(Guid courseId, CancellationToken cancellationToken);
}




