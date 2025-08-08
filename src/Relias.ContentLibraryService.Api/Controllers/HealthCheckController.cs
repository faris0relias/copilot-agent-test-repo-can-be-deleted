using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Relias.ContentLibraryService.Api.Controllers
{
    // API Versioning
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        /// <summary>
        /// Returns a 200 OK response to indicate the service is running.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetHealthStatusAsync()
        {
            return Ok();
        }
    }
}