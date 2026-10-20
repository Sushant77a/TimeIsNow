namespace TimeIsNow.Application.Common.Exceptions;

public sealed class Unauthorized401Exception(string message) : ApiException(message)
{
    public override int StatusCode => 401;
}
