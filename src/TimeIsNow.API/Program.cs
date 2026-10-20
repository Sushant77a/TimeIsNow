using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore;
using TimeIsNow.API.Extensions;
using TimeIsNow.API.Mcp;
using TimeIsNow.API.Middlewares;
using TimeIsNow.API.Security;
using TimeIsNow.Application;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Interfaces.Security;
using TimeIsNow.Infrastructure;
using TimeIsNow.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtOptions = JwtOptions.FromConfiguration(builder.Configuration);

var corsOrigins = CorsExtensions.AllowedOrigins(builder.Configuration);
builder.Services.AddConfiguredCors(corsOrigins);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "name",
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse.Fail(ApiMessages.NotAuthenticated, StatusCodes.Status401Unauthorized));
            },
        };
    });

builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddEnvelopeModelValidation();

builder.Services.AddMcpServer()
    .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless)
    .WithTools<TimeIsNowMcpTools>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(statusCodeContext =>
{
    var context = statusCodeContext.HttpContext;
    var message = ReasonPhrases.GetReasonPhrase(context.Response.StatusCode);
    if (string.IsNullOrEmpty(message))
    {
        message = ApiMessages.UnexpectedError;
    }

    return context.Response.WriteAsJsonAsync(
        ApiResponse.Fail(message, context.Response.StatusCode));
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseConfiguredCors(corsOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMcp("/mcp").RequireAuthorization();
app.MapGet("/api/health", () => Results.Ok(ApiResponse.Success("Healthy", "API is running.")))
    .AllowAnonymous();

app.Run();
