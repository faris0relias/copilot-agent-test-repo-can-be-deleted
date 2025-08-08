using MediatR;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;

public class UpdateLessonCommand
{
    public class Contract : IRequest<LessonDto>
    {
        public required Guid CourseId { get; init; }
        public required Guid SectionId { get; init; }
        public required Guid LearningObjectId { get; init; }
        public required UpdateLessonDto LessonDto { get; init; }
    }

    public class Handler(ILearningContentService service)
        : IRequestHandler<Contract, LessonDto>
    {
        public async Task<LessonDto> Handle(Contract request, CancellationToken cancellationToken)
        {
            var existingLearningContentDto = await service.GetByCourseIdAsync(request.CourseId, cancellationToken)
                ?? throw new InvalidOperationException($"Learning content for course {request.CourseId} not found.");

            return await service.UpdateLessonAsync(request.CourseId, request.SectionId, request.LearningObjectId, request.LessonDto, existingLearningContentDto, cancellationToken);
        }
    }
}
