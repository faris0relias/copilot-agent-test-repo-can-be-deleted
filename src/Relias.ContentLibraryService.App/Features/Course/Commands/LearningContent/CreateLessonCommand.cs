using MediatR;
using Microsoft.Azure.CosmosRepository;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Domain.Course;

namespace Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;

public class CreateLessonCommand
{
    public class Contract : IRequest<LessonDto>
    {
        public required Guid CourseId { get; init; }
        public required Guid SectionId { get; init; }
        public required CreateLessonDto LessonDto { get; init; }
    }

    public class Handler(ILearningContentService service)
        : IRequestHandler<Contract, LessonDto>
    {
        public async Task<LessonDto> Handle(Contract request, CancellationToken cancellationToken)
        {
            var existingLearningContentDto = await service.GetByCourseIdAsync(request.CourseId, cancellationToken)
                ?? throw new InvalidOperationException($"Learning content for course {request.CourseId} not found.");

            return await service.CreateLessonAsync(request.CourseId, request.SectionId, request.LessonDto, existingLearningContentDto, cancellationToken);
        }
    }
}
