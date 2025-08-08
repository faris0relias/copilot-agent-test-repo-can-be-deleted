using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.App.Interfaces.Content;

namespace Relias.ContentLibraryService.App.Services.Content;

public class ContentService(
    IContentRepository contentRepository,
    ICourseRepository courseRepository,
    IMapper mapper) : IContentService
{
    public async Task<IEnumerable<ContentInfoDto>> GetContentByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var fullContentList = new List<ContentInfoDto>();

        if (ids.IsNullOrEmpty())
        {
            return fullContentList;
        }

        var contents = await contentRepository.GetContentByContentIdsAsync(ids, cancellationToken);

        if (contents.IsNullOrEmpty())
        {
            return fullContentList;
        }

        var contentDictionary = contents
            .GroupBy(c => c.ContentTypeId)
            .ToDictionary(g => g.Key, g => g.Select(c => c.ContentId).ToList());

        // Courses; ContentType == 2
        if (contentDictionary.TryGetValue(2, out var courseContentIds) && !courseContentIds.IsNullOrEmpty())
        {
            var courses = await courseRepository.GetCoursesByContentIdsAsync(courseContentIds!, cancellationToken);
            fullContentList.AddRange(courses.Select(mapper.Map<ContentInfoDto>));
        }

        return fullContentList;
    }
}
