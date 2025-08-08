using Microsoft.AspNetCore.Mvc;

namespace Relias.ContentLibraryService.Api.Controllers
{
    [Route("api/[controller]")]
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