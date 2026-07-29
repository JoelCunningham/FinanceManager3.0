namespace FinanceManager.Application.Interfaces;

using FinanceManager.Application.DTOs;

public interface IIdentityService
{
    Task<(Guid? UserId, IEnumerable<string> Errors)> CreateUserAsync(string name, string email, string password);
    Task<string> GenerateEmailConfirmationTokenAsync(string email);
    Task<Guid?> FindByEmailAsync(string email);
    Task<bool> CanSignInAsync(string email);
    Task<(bool Succeeded, bool IsLockedOut)> CheckPasswordAsync(string email, string password);
    Task<string> SignInAsync(Guid? key, string? returnPath);
    Task<string> SignOutAsync();
    Task<UserSummary?> GetCurrentUserAsync();
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<bool> VerifyUserResetTokenAsync(string email, string token);
    Task<(Guid? UserId, IEnumerable<string> Errors)> ResetPasswordAsync(string email, string token, string password);
    Task<Guid?> ConfirmEmailAsync(string email, string token);
    Task<string> GenerateTwoFactorTokenAsync(string email);
    Task<string> GenerateChangeEmailTokenAsync(string email, string newEmail);
    Task<Guid?> ChangeEmailAsync(string email, string newEmail, string token);
    Task<bool> ChangeNameAsync(string email, string newName);
    Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword);
    Task<bool> DeleteUser(string email);
} //TODO replace email with user dto