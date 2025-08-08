using MediatR;
using Microsoft.AspNetCore.Mvc;
using Relias.ContentLibraryService.Api.Versioning;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Features.Content.Queries;
using System.Net;

namespace Relias.ContentLibraryService.Api.Controllers.V1;

[ApiV1]
public class ContentController(IMediator mediator) : ApiControllerBase(mediator)
{
    /// <summary>
    /// Gets all content types.
    /// </summary>
    /// <returns>List of content types.</returns>
    [HttpGet("types")]
    [ProducesResponseType(typeof(List<ContentTypeDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> ListContentTypes(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetContentTypesQuery.Contract(), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets all languages.
    /// </summary>
    /// <returns>List of languages.</returns>
    [HttpGet("languages")]
    [ProducesResponseType(typeof(List<LanguageDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> ListLanguages(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetLanguagesQuery.Contract(), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets Content information based on given ContentIds
    /// </summary>
    /// <returns>List of content.</returns>
    [HttpGet("info")]
    [ProducesResponseType(typeof(List<ContentInfoDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> ListContentInfoByIds([FromHeader(Name = "Content-Ids")] IEnumerable<Guid> contentIds, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetContentByIdsQuery.Contract
        {
            ContentIds = contentIds
        }, cancellationToken);

        return Ok(result);
    }
}
