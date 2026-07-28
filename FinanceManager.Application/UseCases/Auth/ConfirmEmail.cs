namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

public sealed record ConfirmEmailResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class ConfirmEmail(IIndentityService identityService)
{
    public async Task<ConfirmEmailResult> ExecuteAsync(ConfirmEmailModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token))
        {
            return new ConfirmEmailResult([new UseCaseInvalidOperationError("Email and token are required.")]);
        }

        var success = await identityService.ConfirmEmailAsync(model.Email, model.Token);

        if (!success)
        {
            return new ConfirmEmailResult([new UseCaseInvalidOperationError("Email confirmation failed. Invalid token or email.")]);
        }

        return new ConfirmEmailResult([]);
    }
}