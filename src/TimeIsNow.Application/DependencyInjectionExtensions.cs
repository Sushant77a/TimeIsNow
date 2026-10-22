namespace TimeIsNow.Application;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<Interfaces.Services.IAuthService, Services.AuthService>();
        services.AddScoped<Interfaces.Services.ICategoriesService, Services.CategoriesService>();
        services.AddScoped<Interfaces.Services.IProfileService, Services.ProfileService>();
        services.AddScoped<Interfaces.Services.ITodosService, Services.TodosService>();
        services.AddScoped<Interfaces.Services.IDailyEntriesService, Services.DailyEntriesService>();
        services.AddScoped<Interfaces.Services.IJournalService, Services.JournalService>();
        services.AddScoped<Interfaces.Services.IScheduleService, Services.ScheduleService>();
        services.AddScoped<Interfaces.Services.IFinanceService, Services.FinanceService>();
        services.AddScoped<Interfaces.Services.IGoalsService, Services.GoalsService>();
        services.AddScoped<Interfaces.Services.IHabitsService, Services.HabitsService>();
        services.AddScoped<Interfaces.Services.IPracticesService, Services.PracticesService>();
        services.AddScoped<Interfaces.Services.INotesService, Services.NotesService>();
        services.AddScoped<Interfaces.Services.ITagsService, Services.TagsService>();
        services.AddScoped<Interfaces.Services.IDashboardService, Services.DashboardService>();
        services.AddScoped<Interfaces.Services.IModulesService, Services.ModulesService>();
        services.AddScoped<Interfaces.Services.IWeeklyBriefingService, Services.WeeklyBriefingService>();
        services.AddScoped<Services.TemplateNarrativeService>();
        services.AddScoped<Services.TemplatePlanGenerator>();
        return services;
    }
}
