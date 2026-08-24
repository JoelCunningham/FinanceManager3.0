namespace FinanceManager.Application.UseCases.Settings;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;

public sealed record GetProfileResult(ProfileModel Profile, IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class GetProfile(IIdentityService identityService)
{
    public async Task<GetProfileResult> ExecuteAsync()
    {
        var user = await identityService.GetCurrentUserSummaryAsync() ?? throw new InvalidOperationException("User not found.");

        var profile = new ProfileModel(user.Name, user.Email);
        return new GetProfileResult(profile, []);
    }
}
