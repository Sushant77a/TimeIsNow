using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Common.ResponseWrappers;

namespace TimeIsNow.API.Middlewares;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        var statusCode = ResolveStatusCode(exception);

        if (statusCode >= 500)
        {
            logger.LogError(exception, "{Method} {Path} failed", context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(
            ApiResponse.Fail(ResolveMessage(exception, statusCode), statusCode),
            cancellationToken);

        return true;
    }

    private static int ResolveStatusCode(Exception exception) => exception switch
    {
        ApiException apiException => apiException.StatusCode,
        BadHttpRequestException badRequest => badRequest.StatusCode,
        _ => 500,
    };

    private static string ResolveMessage(Exception exception, int statusCode)
    {
        if (exception is ApiException)
            return statusCode < 500 ? exception.Message : ApiMessages.UnexpectedError;

        var reason = statusCode < 500 ? ReasonPhrases.GetReasonPhrase(statusCode) : string.Empty;
        return string.IsNullOrEmpty(reason) ? ApiMessages.UnexpectedError : reason;
    }
}
