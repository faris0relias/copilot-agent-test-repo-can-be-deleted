namespace Relias.ContentLibraryService.Infra.Helpers
{
    public static class FileHelper
    {
        public const string MainContainerName = "main";
        public const string PublicContainerName = "public";

        public static string SanitizePath(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            if (path.StartsWith('/')) path = path[1..];
            if (!path.EndsWith('/')) path += "/";
            return path;
        }
    }
}