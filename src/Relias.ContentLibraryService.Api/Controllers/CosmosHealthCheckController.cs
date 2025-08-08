using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace Relias.ContentLibraryService.Api.Controllers;

[Route("api/[controller]")]
public class CosmosHealthCheckController : ControllerBase
{
    private readonly CosmosClient _cosmosClient;
    private readonly IConfiguration _appSettings;

    public CosmosHealthCheckController(IConfiguration appsettings, CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
        _appSettings = appsettings;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var _response = await _cosmosClient.GetDatabase(_appSettings["CosmosRepositoryOptions:DatabaseId"]).ReadAsync();
            if (_response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok("Cosmos DB: Ok");
            }
            return StatusCode(503, "Failed to connect Cosmos DB");
        }
        catch
        {
            return StatusCode(503, "Failed to connect Cosmos DB");
        }
    }
}
