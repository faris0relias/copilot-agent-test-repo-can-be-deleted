namespace Relias.ContentLibraryService.Common.Services
{
    public interface IHealthCheckService
    {
        Task<Tuple<bool, string>> GetHealthStatusAsync();
    }
}