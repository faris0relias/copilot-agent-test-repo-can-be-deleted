namespace Relias.ContentLibraryService.Common.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException()
        : base()
    {
    }

    public BadRequestException(string message)
        : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public BadRequestException(string value, string type, string reason)
        : base($"{type} '{value}' is invalid. Reason: {reason}")
    {
    }
}