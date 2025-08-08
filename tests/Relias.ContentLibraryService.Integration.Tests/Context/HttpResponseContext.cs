using Reqnroll;

namespace Relias.ContentLibraryService.Integration.Tests.Context;

[Binding]
public class HttpResponseContext
{
    public HttpResponseMessage? Response { get; set; }
}
