using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Queries;

public class GetLearningContentByCourseIdQuery
{
    public class Contract : IRequest<LearningContentDto?>
    {
        public Guid CourseId { get; init; }
    }

    public class Handler(ILearningContentService service)
        : IRequestHandler<Contract, LearningContentDto?>
    {

        public async Task<LearningContentDto?> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await service.GetByCourseIdAsync(contract.CourseId, cancellationToken);
        }
    }
}

