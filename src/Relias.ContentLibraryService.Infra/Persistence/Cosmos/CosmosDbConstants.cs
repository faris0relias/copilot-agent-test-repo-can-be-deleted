namespace Relias.ContentLibraryService.Infra.Persistence.Cosmos;

public static class CosmosDbConstants
{
    public const string DatabaseName = "ContentLibraryServiceCosmosDb";

    public static class ContainerNames
    {
        public const string FinalExam = "FinalExam";
        public const string LearningContent = "LearningContent";
    }
}