using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Security;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Application.Services;
using TimeIsNow.Infrastructure.Ai;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Infrastructure.Repositories;
using TimeIsNow.Infrastructure.Security;

namespace TimeIsNow.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TimeIsNow")
            ?? throw new InvalidOperationException(
                "Connection string 'TimeIsNow' not found. " +
                "Set it with: dotnet user-secrets set \"ConnectionStrings:TimeIsNow\" \"<value>\"");

        services.AddDbContext<TimeIsNowDbContext>(options =>
            options.UseMySQL(connectionString));

        services.AddSingleton(JwtOptions.FromConfiguration(configuration));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<IDailyEntryRepository, DailyEntryRepository>();
        services.AddScoped<IJournalRepository, JournalRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IFinanceRepository, FinanceRepository>();
        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<IHabitRepository, HabitRepository>();
        services.AddScoped<IPracticeRepository, PracticeRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<ICategoryModuleRepository, CategoryModuleRepository>();

        var aiOptions = AiOptions.FromConfiguration(configuration);
        if (!string.IsNullOrWhiteSpace(aiOptions.ApiKey))
        {
            services.AddSingleton(aiOptions);
            if (aiOptions.Protocol == AiProtocol.Anthropic)
                services.AddScoped<IAiChatClient, AnthropicChatClient>();
            else
                services.AddScoped<IAiChatClient, OpenAiChatClient>();
            services.AddScoped<INarrativeService, AiNarrativeService>();
            services.AddScoped<IPracticePlanGenerator, AiPlanGenerator>();
        }
        else
        {
            services.AddScoped<INarrativeService, TemplateNarrativeService>();
            services.AddScoped<IPracticePlanGenerator, TemplatePlanGenerator>();
        }

        return services;
    }
}
