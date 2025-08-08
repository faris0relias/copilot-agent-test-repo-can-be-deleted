using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Queries;

public class GetCoursesQuery
{
    public class Contract : IRequest<IEnumerable<CourseDto>?>
    {
        public int OrganizationId { get; set; }
    }

    public class Handler(ICourseService courseService)
        : IRequestHandler<Contract, IEnumerable<CourseDto>?>
    {
        public async Task<IEnumerable<CourseDto>?> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await courseService.GetCoursesAsync(contract.OrganizationId, cancellationToken);
        }
    }
}
