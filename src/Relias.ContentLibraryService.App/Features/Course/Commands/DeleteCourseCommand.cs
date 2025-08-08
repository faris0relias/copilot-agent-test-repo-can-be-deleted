using MediatR;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands;

public static class DeleteCourseCommand
{
    public class Contract(Guid courseId) : IRequest
    {
        public Guid CourseId { get; } = courseId;
    }

    public class Handler(ICourseService courseService) : IRequestHandler<Contract>
    {
        public async Task Handle(Contract contract, CancellationToken cancellationToken)
        {            
            await courseService.DeleteAsync(contract.CourseId, cancellationToken);
        }
    }
}