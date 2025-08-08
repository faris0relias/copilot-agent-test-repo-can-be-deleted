using Relias.ContentLibraryService.Common;

namespace Relias.ContentLibraryService.Common.Services
{
    public interface IDomainEventService
    {
        Task Publish(DomainEvent domainEvent);

    }
}