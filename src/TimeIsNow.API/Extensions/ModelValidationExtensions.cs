using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;

namespace TimeIsNow.API.Extensions;

public static class ModelValidationExtensions
{
    public const int ValidationFailedStatus = StatusCodes.Status400BadRequest;

    public static IMvcBuilder AddEnvelopeModelValidation(this IMvcBuilder builder) =>
        builder.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                return CreateResponse(
                    context.ModelState,
                    context.ActionDescriptor.Parameters
                        .Where(parameter => parameter.BindingInfo?.BindingSource == BindingSource.Body)
                        .Select(parameter => parameter.Name));
            };

            options.SuppressMapClientErrors = true;
        });

    public static ObjectResult CreateResponse(ModelStateDictionary modelState, IEnumerable<string>? bodyParameterNames = null)
    {
        var bodyParameters = new HashSet<string>(bodyParameterNames ?? [], StringComparer.OrdinalIgnoreCase);
        var errors = new Dictionary<string, string[]>();
        var bodyParameterMissing = false;

        foreach (var (key, entry) in modelState)
        {
            if (entry is not { Errors.Count: > 0 })
            {
                continue;
            }

            if (bodyParameters.Contains(key))
            {
                bodyParameterMissing = true;
                continue;
            }

            var field = FieldName(key);
            var messages = SafeMessages(key, entry);
            errors[field] = errors.TryGetValue(field, out var existing) ? [.. existing, .. messages] : messages;
        }

        if (bodyParameterMissing && errors.Count == 0)
        {
            errors["request"] = [ApiMessages.RequestBodyRequired];
        }

        var message = errors.Values
            .SelectMany(messages => messages)
            .FirstOrDefault(text => text != ApiMessages.InvalidValue)
            ?? ApiMessages.ValidationFailed;

        return new ObjectResult(ApiResponse.Fail(message, ValidationFailedStatus, errors))
        {
            StatusCode = ValidationFailedStatus,
        };
    }

    private static string[] SafeMessages(string key, ModelStateEntry entry) =>
        key.StartsWith('$')
            ? [ApiMessages.InvalidValue]
            : [.. entry.Errors.Select(error =>
                error.Exception is null && !string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? error.ErrorMessage
                    : ApiMessages.InvalidValue)];

    private static string FieldName(string key)
    {
        var path = key.TrimStart('$', '.');

        return path.Length == 0
            ? "request"
            : string.Join('.', path.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
    }
}
