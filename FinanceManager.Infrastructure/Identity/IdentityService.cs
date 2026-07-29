namespace FinanceManager.Infrastructure.Identity;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

public class IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILoginTicketStore ticketStore, AuthenticationStateProvider authStateProvider) : IIdentityService
{
    public async Task<(Guid? UserId, IEnumerable<string> Errors)> CreateUserAsync(string name, string email, string password)
    {
        var user = new ApplicationUser(name, email);
        var result = await userManager.CreateAsync(user, password);
        return (result.Succeeded ? Guid.Parse(user.Id) : null, [.. result.Errors.Select(e => e.Description)]);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email) ?? throw new Exception("User not found");
        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<Guid?> FindByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user?.Id is not null ? Guid.Parse(user.Id) : null;
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

    public async Task<UserSummary?> GetCurrentUserAsync()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;

        if (principal?.Identity?.IsAuthenticated != true) return null;

        var user = await userManager.GetUserAsync(principal);

        if (user is null) return null;

        return new UserSummary(user.Email!, user.Name);
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

    public async Task<(Guid? UserId, IEnumerable<string> Errors)> ResetPasswordAsync(string email, string token, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return (null, new[] { "User not found" });
        var result = await userManager.ResetPasswordAsync(user, token, password);
        return (result.Succeeded ? Guid.Parse(user.Id) : null, [.. result.Errors.Select(e => e.Description)]);
    }

    public async Task<Guid?> ConfirmEmailAsync(string email, string token)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return null;
        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded ? Guid.Parse(user.Id) : null;
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

    public async Task<Guid?> ChangeEmailAsync(string email, string newEmail, string token)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return null;
        var result = await userManager.ChangeEmailAsync(user, newEmail, token);
        result = result.Succeeded ? await userManager.SetUserNameAsync(user, newEmail) : result;
        return result.Succeeded ? Guid.Parse(user.Id) : null;
    }

    public async Task<bool> ChangeNameAsync(string email, string newName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return false;
        user.Name = newName;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return false;
        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUser(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return false;
        var result = await userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}