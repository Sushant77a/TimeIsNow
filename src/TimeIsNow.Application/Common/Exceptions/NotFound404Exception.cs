namespace TimeIsNow.Application.Common.Exceptions;

public sealed class NotFound404Exception(string message) : ApiException(message)
{
    public override int StatusCode => 404;
}
