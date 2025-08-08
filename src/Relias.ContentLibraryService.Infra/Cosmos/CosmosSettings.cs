namespace Relias.ContentLibraryService.Infra.Cosmos;

public class CosmosSettings
{
    internal const string ConfigSection = "AppSettings:CosmosRepositoryOptions";

    public string AccountEndpoint { get; set; } = string.Empty;
    public string DatabaseId { get; set; } = string.Empty;
    public string CosmosConnectionString { get; set; } = string.Empty;
}
