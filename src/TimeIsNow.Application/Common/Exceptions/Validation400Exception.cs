namespace TimeIsNow.Application.Common.Exceptions;

public sealed class Validation400Exception(string message) : ApiException(message)
{
    public override int StatusCode => 400;
}
