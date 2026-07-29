namespace FinanceManager.Application.UseCases.Settings;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record SendMfaCodeResult(string MfaCode, IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class SendMfaCode(IUserEmailService emailService, IIdentityService identityService)
{
    public async Task<SendMfaCodeResult> ExecuteAsync(string name, string address)
    {
        var mfaCode = await identityService.GenerateTwoFactorTokenAsync(address);
        await emailService.SendMfaCodeAsync(name, address, mfaCode);

        return new SendMfaCodeResult(mfaCode, []);
    }
}
