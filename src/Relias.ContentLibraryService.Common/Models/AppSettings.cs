using System.Diagnostics.CodeAnalysis;

namespace Relias.ContentLibraryService.Common.Models;

[ExcludeFromCodeCoverage]
public class AppSettings
{
    public AzureB2C AzureB2C { get; set; } = null!;
    public string SqlServerConnectionString { get; set; } = null!;
    public MalwareScannerOptions MalwareScannerOptions { get; set; } = null!;
    
    public string DatabaseId { get; set; } = null!;
    public string CosmosConnectionString { get; set; } = null!;
    public string AccountEndpoint { get; set; } = null!;
    public string StorageAccount { get; set; } = null!;
    public string Identity { get; set; } = null!;
}

public class AzureB2C
{
    public string ClientId { get; set; } = null!;
}