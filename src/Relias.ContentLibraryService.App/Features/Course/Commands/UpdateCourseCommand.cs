using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands;

public static class UpdateCourseCommand
{
    public class Contract : IRequest<CourseDto>
    {
        public required UpdateCourseDto UpdatedCourse { get; init; }
        public required Guid CourseId { get; set; }
    }

    public class Handler(ICourseService courseService)
        : IRequestHandler<Contract, CourseDto>
    {
        public async Task<CourseDto> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await courseService.UpdateAsync(contract.CourseId, contract.UpdatedCourse, cancellationToken);
        }
    }
}
