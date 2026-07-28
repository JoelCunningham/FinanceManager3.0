namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Interfaces;
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
        [FromServices] IIndentityService identityService)
    {
        return Results.Redirect(await identityService.SignInAsync(key, returnPath));
    }

    private static async Task<IResult> SignOutAsync([FromServices] IIndentityService identityService)
    {
        return Results.Redirect(await identityService.SignOutAsync());
    }
}
