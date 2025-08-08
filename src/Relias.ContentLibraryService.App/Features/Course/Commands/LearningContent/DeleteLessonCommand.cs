using MediatR;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;

public static class DeleteLessonCommand
{
    public class Contract(Guid courseId, Guid sectionId, Guid learningObjectId, int orgId) : IRequest
    {
        public Guid CourseId { get; } = courseId;
        public Guid SectionId { get; } = sectionId;
        public Guid LearningObjectId { get; } = learningObjectId;
        public int OrgId { get; } = orgId;
    }

    public class Handler(ILearningContentService service) : IRequestHandler<Contract>
    {
        public async Task Handle(Contract request, CancellationToken cancellationToken)
        {
            var existingLearningContentDto = await service.GetByCourseIdAsync(request.CourseId, cancellationToken)
                ?? throw new InvalidOperationException($"Learning content for course {request.CourseId} not found.");

            await service.DeleteLessonAsync(request.CourseId, request.SectionId, request.LearningObjectId, request.OrgId, existingLearningContentDto, cancellationToken);
        }
    }
}
