using MediatR;
using Microsoft.AspNetCore.Http;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;

public class UploadLessonFileCommand
{
    public class Contract : IRequest<Unit>
    {
        public required Guid CourseId { get; init; }
        public required Guid LearningObjectId { get; init; }
        public required int OrganizationId { get; init; }
        public required IFormFile Chunk { get; init; }
        public required string UploadId { get; init; }
        public required int ChunkIndex { get; init; }
        public required int TotalChunks { get; init; }
        public required string FileName { get; init; }
    }

    public class Handler(ILearningContentService service) : IRequestHandler<Contract, Unit>
    {
        public async Task<Unit> Handle(Contract request, CancellationToken cancellationToken)
        {
            await service.UploadLessonFileAsync(
                request.OrganizationId,
                request.CourseId,
                request.LearningObjectId,
                request.Chunk,
                request.UploadId,
                request.ChunkIndex,
                request.TotalChunks,
                request.FileName,
                cancellationToken);

            return Unit.Value;
        }
    }
}
