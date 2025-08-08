using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relias.ContentLibraryService.Api.Filters;

namespace Relias.ContentLibraryService.Api.Controllers;

/// <summary>
/// API controller base class which holds an instance of <see cref="ISender"/> Mediatr service
/// </summary>
[ApiController]
[Authorize]
[ApiExceptionFilter]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class ApiControllerBase(IMediator mediator) : ControllerBase
{
    protected readonly IMediator Mediator = mediator;
}