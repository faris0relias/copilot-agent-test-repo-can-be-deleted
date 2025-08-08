using MediatR;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.Common.Models;

namespace Relias.ContentLibraryService.Common.Services
{
    public class DomainEventService(ILogger<DomainEventService> logger, IPublisher mediator) : IDomainEventService
    {
        public Task Publish(DomainEvent domainEvent)
        {
            logger.LogInformation("Publishing domain event. Event - {event}", domainEvent.GetType().Name);
            return mediator.Publish(GetNotificationCorrespondingToDomainEvent(domainEvent));
        }

        private INotification GetNotificationCorrespondingToDomainEvent(DomainEvent domainEvent)
        {
            return (INotification)Activator.CreateInstance(
                typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType()), domainEvent)!;
        }
    }
}