namespace Relias.ContentLibraryService.Common.Services
{
    public interface IDependencyHealthCheck
    {
        public string ServiceName { get; }

        public Task CheckServiceHealth();
    }
}