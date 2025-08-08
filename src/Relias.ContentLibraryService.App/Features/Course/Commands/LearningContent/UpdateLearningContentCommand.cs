using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;

public class UpdateLearningContentCommand
{
    public class Contract : IRequest<LearningContentDto>
    {
        public Guid CourseId { get; init; }
        public required JsonPatchDocument Updates { get; init; }
    }

    public class Handler(ILearningContentService service) : IRequestHandler<Contract, LearningContentDto>
    {
        public async Task<LearningContentDto> Handle(Contract request, CancellationToken cancellationToken)
        {
            var existingLearningContentDto = await service.GetByCourseIdAsync(request.CourseId, cancellationToken)
                ?? throw new InvalidOperationException($"Learning content for course {request.CourseId} not found.");

            return await service.UpdateLearningContentAsync(request.CourseId,request.Updates, existingLearningContentDto, cancellationToken);
        }
    }
}