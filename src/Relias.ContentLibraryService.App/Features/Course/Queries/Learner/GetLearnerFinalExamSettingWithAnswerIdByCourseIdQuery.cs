using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Queries;

public sealed class GetLearnerFinalExamSettingWithAnswerIdByCourseIdQuery
{
    public class Contract : IRequest<FinalExamSettingWithAnswerDto?>
    {
        public Guid CourseId { get; init; }
    }

    public class Handler(IFinalExamService service) : IRequestHandler<Contract, FinalExamSettingWithAnswerDto?>
    {
        public async Task<FinalExamSettingWithAnswerDto?> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await service.GetLearnerFinalExamQuestionAnswerIdsAsync(contract.CourseId, cancellationToken);
        }
    }
}