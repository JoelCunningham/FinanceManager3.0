namespace FinanceManager.Application.UseCases.Settings;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

public sealed record UpdateUserEmailResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class UpdateUserEmail(IIdentityService identityService, IUserEmailService emailService, IAuditLogRepository auditLog, IDataStore dataStore)
{
    public async Task<UpdateUserEmailResult> ExecuteAsync(ProfileModel model)
    {
        if (model.CurrentEmail == model.NewEmail) return new UpdateUserEmailResult([]);

        var currentUser = await identityService.FindByEmailAsync(model.CurrentEmail) ?? throw new InvalidOperationException("Current user not found.");

        var existingUser = await identityService.FindByEmailAsync(model.NewEmail);
        if (existingUser is not null)
        {
            return new UpdateUserEmailResult([new UseCaseInvalidOperationError("The provided email is already taken.")]);
        }

        var token = await identityService.GenerateChangeEmailTokenAsync(model.CurrentEmail, model.NewEmail);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var confirmationLink = QueryHelpers.AddQueryString($"{model.Origin}{Pages.ConfirmEmail}", new Dictionary<string, string?>
        {
            [Parameters.Token] = encodedToken,
            [Parameters.Email] = model.NewEmail,
            [Parameters.OldEmail] = model.CurrentEmail,
            [Parameters.Reason] = ConfirmEmailReason.UpdateEmail.ToString()
        });

        await emailService.SendEmailUpdateConfirmationAsync(model.NewName, model.NewEmail, confirmationLink);

        await auditLog.LogAsync(currentUser, AuditedEvent.EmailChangeRequested, $"Email {model.NewEmail} has been requested for change.", DateTime.Now);
        await dataStore.SaveAsync();

        return new UpdateUserEmailResult([]);
    }
}
