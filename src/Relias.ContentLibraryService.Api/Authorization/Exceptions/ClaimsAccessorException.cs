namespace Relias.ContentLibraryService.Api.Authorization.Exceptions;

public class ClaimsAccessorException : Exception
{
    public ClaimsAccessorException(string userTokenKey)
        : base("Unable to determine token key from current HTTP Context")
    {
        Data["UserTokenKey"] = userTokenKey;
    }
}
