using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands;

public class PublishCourseCommand
{
    public class Contract : IRequest<Unit>
    {
        public required PublishCourseDto PublishCourse { get; init; }
        public required Guid CourseId { get; set; }
    }

    public class Handler(ICourseService courseService) : IRequestHandler<Contract, Unit>
    {
        public async Task<Unit> Handle(Contract contract, CancellationToken cancellationToken)
        {
            await courseService.PublishAsync(contract.CourseId, contract.PublishCourse, cancellationToken);
            return Unit.Value;
        }
    }
}
