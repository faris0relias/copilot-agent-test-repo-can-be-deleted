using Reqnroll;
using System.Net.Http.Headers;

namespace Relias.ContentLibraryService.Integration.Tests.Context;

[Binding]
public class AuthenticationHeaderContext
{
    public AuthenticationHeaderValue? AuthenticationHeader { get; set; }
}