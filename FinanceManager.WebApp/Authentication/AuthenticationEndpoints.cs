namespace FinanceManager.WebApp.Authentication;

using FinanceManager.Infrastructure.Identity;
using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Pages.AuthLogin, SignInAsync);
        endpoints.MapGet(Pages.AuthLogout, SignOutAsync);

        return endpoints;
    }

    private static async Task<IResult> SignInAsync(
        [FromQuery(Name = Parameters.Key)] Guid? key,
        [FromQuery(Name = Parameters.ReturnPath)] string? returnPath,
        [FromServices] PendingLoginStore store,
        [FromServices] SignInManager<ApplicationUser> signInManager)
    {
        if (key is null)
        {
            return Results.NotFound();
        }
        if (!store.TryRemove(key.Value, out var login))
        {
            return Results.BadRequest();
        }
        if (login is null)
        {
            return Results.BadRequest();
        }

        var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, login.RememberMe, false);

        if (!result.Succeeded)
        {
            return Results.Redirect(Pages.AuthLogin);
        }

        return Results.Redirect(returnPath is not null ? "/" + returnPath : Pages.Dashboard);
    }

    private static async Task<IResult> SignOutAsync(
        [FromServices] SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.Redirect(Pages.Login);
    }
}
