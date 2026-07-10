namespace FinanceManager.WebApp.Authentication;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddSingleton<PendingLoginStore>();
        services.AddScoped<AuthenticationService>();

        return services;
    }
}