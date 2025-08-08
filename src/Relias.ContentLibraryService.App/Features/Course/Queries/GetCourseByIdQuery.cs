using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Queries;

public class GetCourseByIdQuery
{
    public class Contract : IRequest<CourseDto?>
    {
        public Guid CourseId { get; init; }
    }

    public class Handler(ICourseService service)
        : IRequestHandler<Contract, CourseDto?>
    {

        public async Task<CourseDto?> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await service.GetByIdAsync(contract.CourseId, cancellationToken);
        }
    }
}
