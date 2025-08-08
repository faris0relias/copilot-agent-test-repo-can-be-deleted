using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.Domain.Course.LearningContent;

namespace Relias.ContentLibraryService.Integration.Tests.Context;

public class LearningContentContext
{
    public LearningContent? LearningContent { get; set; }
    public LearningContentDto? LearningContentDto { get; set; }
}
