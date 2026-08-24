namespace FinanceManager.Infrastructure.Identity;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

public class IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILoginTicketStore ticketStore, AuthenticationStateProvider authStateProvider) : IIdentityService
{
    public async Task<(UserSummary? User, IEnumerable<string> Errors)> CreateUserAsync(string name, string email, string password)
    {
        var user = new ApplicationUser(name, email);
        var result = await userManager.CreateAsync(user, password);
        return (IdentityUserResult(result, user), IdentityUserErrors(result));
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email) ?? throw new Exception("User not found");
        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<UserSummary?> FindByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user?.ToUserSummary();
    }

    public async Task<bool> CanSignInAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is not null && await signInManager.CanSignInAsync(user);
    }

    public async Task<(bool Succeeded, bool IsLockedOut)> CheckPasswordAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return (false, false);

        var result = await signInManager.CheckPasswordSignInAsync(user, password, true);
        return (result.Succeeded, result.IsLockedOut);
    }

    public async Task<string> SignInAsync(Guid? key, string? returnPath)
    {
        if (key is null || !ticketStore.TryConsume(key.Value, out var ticket) || ticket is null)
        {
            return Pages.Login;
        }

        var user = await userManager.FindByEmailAsync(ticket.Email) ?? throw new Exception("User not found");
        await signInManager.SignInAsync(user, ticket.RememberMe);

        return !string.IsNullOrWhiteSpace(returnPath) ? "/" + returnPath : Pages.Dashboard;
    }

    public async Task<string> SignOutAsync()
    {
        await signInManager.SignOutAsync();
        return Pages.Login;
    }

    public async Task<UserSummary?> GetCurrentUserSummaryAsync()
    {
        return (await GetCurrentUserAsync())?.ToUserSummary();
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email) ?? throw new Exception("User not found");
        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> VerifyUserResetTokenAsync(string email, string token)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return false;
        return await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token);
    }

    public async Task<(UserSummary? User, IEnumerable<string> Errors)> ResetPasswordAsync(string email, string token, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return (null, new[] { "User not found" });
        var result = await userManager.ResetPasswordAsync(user, token, password);
        return (IdentityUserResult(result, user), IdentityUserErrors(result));
    }

    public async Task<UserSummary?> ConfirmEmailAsync(string email, string token)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return null;
        var result = await userManager.ConfirmEmailAsync(user, token);
        return IdentityUserResult(result, user);
    }

    public async Task<string> GenerateTwoFactorTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email) ?? throw new Exception("User not found");
        return await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
    }

    public async Task<string> GenerateChangeEmailTokenAsync(string email, string newEmail)
    {
        var user = await userManager.FindByEmailAsync(email) ?? throw new Exception("User not found");
        return await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
    }

    public async Task<UserSummary?> ChangeEmailAsync(string newEmail, string token)
    {
        var user = await GetCurrentUserAsync() ?? throw new Exception("Current user not found");
        var result = await userManager.ChangeEmailAsync(user, newEmail, token);
        result = result.Succeeded ? await userManager.SetUserNameAsync(user, newEmail) : result;
        return IdentityUserResult(result, user);
    }

    public async Task<UserSummary?> ChangeNameAsync(string newName)
    {
        var user = await GetCurrentUserAsync() ?? throw new Exception("Current user not found");
        user.Name = newName;
        var result = await userManager.UpdateAsync(user);
        return IdentityUserResult(result, user);
    }

    public async Task<UserSummary?> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var user = await GetCurrentUserAsync() ?? throw new Exception("Current user not found");
        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return IdentityUserResult(result, user);
    }

    public async Task<UserSummary?> DeleteUser()
    {
        var user = await GetCurrentUserAsync() ?? throw new Exception("Current user not found");
        var result = await userManager.DeleteAsync(user);
        return IdentityUserResult(result, user);
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;

        if (principal?.Identity?.IsAuthenticated != true) return null;

        return await userManager.GetUserAsync(principal);
    }

    private static UserSummary? IdentityUserResult(IdentityResult result, ApplicationUser user)
    {
        return result.Succeeded ? user.ToUserSummary() : null;
    }

    private static IEnumerable<string> IdentityUserErrors(IdentityResult result)
    {
        return [.. result.Errors.Select(e => e.Description)];
    }
}