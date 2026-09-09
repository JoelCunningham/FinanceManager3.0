namespace FinanceManager.Infrastructure.Identity;

using FinanceManager.Application.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public class CurrentUserService(AuthenticationStateProvider authStateProvider) : ICurrentUserService
{
    public Guid? UserId => GetUserId().Result;

    private async Task<Guid?> GetUserId()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;

        if (principal?.Identity?.IsAuthenticated != true) return null;
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

       return userId == null ? null : Guid.Parse(userId);
    }
}