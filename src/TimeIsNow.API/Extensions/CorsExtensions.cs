namespace TimeIsNow.API.Extensions;

public static class CorsExtensions
{
    public static string[] AllowedOrigins(IConfiguration configuration) =>
        configuration.GetSection("Cors").GetSection("AllowedOrigins").Get<string[]>() ?? [];

    public static IServiceCollection AddConfiguredCors(this IServiceCollection services, string[] origins)
    {
        if (origins.Length == 0)
        {
            return services;
        }

        return services.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            if (origins.Contains("*"))
            {
                policy.AllowAnyOrigin();
            }
            else
            {
                policy.WithOrigins(origins).AllowCredentials();
            }

            policy.AllowAnyHeader().AllowAnyMethod();
        }));
    }

    public static WebApplication UseConfiguredCors(this WebApplication app, string[] origins)
    {
        if (origins.Length > 0)
        {
            app.UseCors();
        }

        return app;
    }
}
