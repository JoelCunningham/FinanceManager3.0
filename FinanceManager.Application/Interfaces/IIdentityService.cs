namespace FinanceManager.Application.Interfaces;

using FinanceManager.Application.DTOs;

public interface IIdentityService
{
    Task<(UserSummary? User, IEnumerable<string> Errors)> CreateUserAsync(string name, string email, string password);
    Task<string> GenerateEmailConfirmationTokenAsync(string email);
    Task<UserSummary?> FindByEmailAsync(string email);
    Task<bool> CanSignInAsync(string email);
    Task<(bool Succeeded, bool IsLockedOut)> CheckPasswordAsync(string email, string password);
    Task<string> SignInAsync(Guid? key, string? returnPath);
    Task<string> SignOutAsync();
    Task<UserSummary?> GetCurrentUserSummaryAsync();
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<bool> VerifyUserResetTokenAsync(string email, string token);
    Task<(UserSummary? User, IEnumerable<string> Errors)> ResetPasswordAsync(string email, string token, string password);
    Task<UserSummary?> ConfirmEmailAsync(string email, string token);
    Task<string> GenerateTwoFactorTokenAsync(string email);
    Task<string> GenerateChangeEmailTokenAsync(string email, string newEmail);
    Task<UserSummary?> ChangeEmailAsync(string newEmail, string token);
    Task<UserSummary?> ChangeNameAsync(string newName);
    Task<UserSummary?> ChangePasswordAsync(string currentPassword, string newPassword);
    Task<UserSummary?> DeleteUser();
}