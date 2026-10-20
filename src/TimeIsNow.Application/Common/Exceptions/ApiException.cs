namespace TimeIsNow.Application.Common.Exceptions;

public abstract class ApiException(string message) : Exception(message)
{
    public abstract int StatusCode { get; }
}
