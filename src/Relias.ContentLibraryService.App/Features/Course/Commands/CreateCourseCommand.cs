using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands;

public class CreateCourseCommand
{
   public class Contract : IRequest<CourseDto> 
    {
        public required CreateCourseDto NewCourse { get; init; }
    }

    public class Handler(ICourseService courseService)
        : IRequestHandler<Contract, CourseDto>
    {
        public async Task<CourseDto> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await courseService.CreateCourseAsync(contract.NewCourse, cancellationToken);
        }
    }
}
