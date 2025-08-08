namespace Relias.ContentLibraryService.Consumer.Models
{
    public class BaseSettings
    {
        public string SqlServerConnectionString { get; set; } = null!;
        public string ServiceBusConnectionString { get; set; } = null!;
        public string PolicyPublishedEventTopicName { get; set; } = null!;
        public string PolicyPublishedEventSubscription { get; set; } = null!;
        public string PolicyUpdatedEventTopicName { get; set; } = null!;
        public string PolicyUpdatedEventSubscription { get; set; } = null!;
        public string PolicyArchivedEventTopicName { get; set; } = null!;
        public string PolicyArchivedEventSubscription { get; set; } = null!;
        public string ContentChangedEventTopicName { get; set; } = null!;
        public string AssignContentEventTopicName { get; set; } = null!;
        public string AssignContentEventSubscription { get; set; } = null!;
        public string PolicyContentAssignedEventTopicName { get; set; } = null!;
        public string PolicyAttestedEventTopicName { get; set; } = null!;
        public string PolicyAttestedEventSubscription { get; set; } = null!;
        public string ContentCompletedEventTopicName { get; set; } = null!;
        public string PolicySyncEventTopicName { get; set; } = null!;
        public string PolicySyncEventSubscription { get; set; } = null!;
        public string ContentArchivedEventTopicName {get;set;} = null!;
    }
}
