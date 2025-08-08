using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Queries.Learner;

public sealed class GetLearnerFinalExamSettingQuery
{
    public class Contract : IRequest<FinalExamSettingDto?>
    {
        public Guid CourseId { get; init; }
    }

    public class Handler(IFinalExamService service) : IRequestHandler<Contract, FinalExamSettingDto?>
    {
        public async Task<FinalExamSettingDto?> Handle(Contract contract, CancellationToken cancellationToken)
        {
                return await service.GetFinalExamSettingsAsync(contract.CourseId, cancellationToken);   
        }
    }
}