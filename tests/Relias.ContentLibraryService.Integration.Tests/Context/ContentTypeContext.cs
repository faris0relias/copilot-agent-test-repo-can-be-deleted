using Relias.ContentLibraryService.Domain.ContentType;
namespace Relias.ContentLibraryService.Integration.Tests.Context;
public class ContentTypeContext
{
    public ContentType? ContentType { get; set; }
    public List<ContentType>? ContentTypes { get; set; }
}