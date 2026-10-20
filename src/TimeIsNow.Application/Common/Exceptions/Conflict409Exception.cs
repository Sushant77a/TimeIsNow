namespace TimeIsNow.Application.Common.Exceptions;

public sealed class Conflict409Exception(string message) : ApiException(message)
{
    public override int StatusCode => 409;
}
