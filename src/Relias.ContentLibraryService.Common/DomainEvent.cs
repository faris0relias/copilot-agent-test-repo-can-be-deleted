namespace Relias.ContentLibraryService.Common
{
    public abstract class DomainEvent
    {
        public bool IsPublished { get; set; }
        public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;
    }
    
    public interface IHasDomainEvent
    {
        public List<DomainEvent> DomainEvents { get; }
    }
}