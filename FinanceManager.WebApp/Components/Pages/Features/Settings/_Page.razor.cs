namespace FinanceManager.WebApp.Components.Pages.Features.Settings;

using FinanceManager.Application.Common;
using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Models;
using FinanceManager.Domain.Enums;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Components.Shared.Wrappers;
using FinanceManager.WebApp.Utilities;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

[Route(Pages.Settings)]
public partial class _Page : MainPageBase
{
    [Inject] public UserState UserState { get; set; } = default!;

    private ProfileModel ProfileModel { get; set; } = new();
    private PasswordModel PasswordModel { get; set; } = new();
    private OptionsModel OptionsModel { get; set; } = new();

    private MfaModal MfaModal { get; set; } = new();
    private Confirmation Confirmation { get; set; } = new();

    private Func<Task>? PendingAction { get; set; }
    private bool HasAuthenticated { get; set; }

    private string MfaCode { get; set; } = string.Empty;
    private bool MfaCodeError { get; set; }

    private bool HasChangedEmail { get; set; }

    private Task HandleProfileUpdate()
    {
        if (ProfileModel.NewEmail != ProfileModel.CurrentEmail)
        {
            return RequireAuthentication(UpdateEmailAsync);
        }
        if (ProfileModel.NewName != ProfileModel.CurrentName)
        {
            return RequireAuthentication(UpdateNameAsync);
        }
        return Task.CompletedTask;
    }
    private Task HandlePasswordUpdate() => RequireAuthentication(UpdatePasswordAsync);
    private Task HandleUserDelete() => RequireAuthentication(DeleteUserAsync);

    private async Task HandleOptionsUpdate()
    {
        await Preferences.Set(PreferenceNames.PerferedColourMode, OptionsModel.PreferredColourMode);
        await ThemeUtilities.UpdateTheme(OptionsModel.PreferredColourMode, JS, UserState);

        Validation.SetSuccess("Your colour mode has been updated successfully.");
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        ProfileModel = (await UseCases.GetProfileAsync()).Profile;
        OptionsModel.PreferredColourMode = await Preferences.PerferedColourMode;
    }

    private async Task RequireAuthentication(Func<Task> action)
    {
        if (HasAuthenticated)
        {
            await action();
            return;
        }

        PendingAction = action;

        await GenerateNewMfaCode();
        await MfaModal.ShowAsync();
    }

    private async Task GenerateNewMfaCode()
    {
        MfaCode = (await UseCases.SendMfaCodeAsync(ProfileModel.CurrentName, ProfileModel.CurrentEmail)).MfaCode;
    }

    private async Task HandleMfaSubmit(string code)
    {
        MfaCodeError = false;

        if (code.Trim() == MfaCode)
        {
            HasAuthenticated = true;

            if (PendingAction is not null)
            {
                var action = PendingAction;
                PendingAction = null;
                await action();
            }
        }
        else
        {
            MfaCodeError = true;
        }
    }

    private async Task UpdateEmailAsync()
    {
        ProfileModel.Origin = Navigation.BaseUri.TrimEnd('/');
        var result = await UseCases.UpdateUserEmailAsync(ProfileModel);

        await MfaModal.HideAsync();

        if (result.Errors.Any())
        {
            Validation.SetErrors(result.Errors);
            ProfileModel.NewEmail = ProfileModel.CurrentEmail;
        }
        else
        {
            HasChangedEmail = true;
            await MessageBox.ShowAsync("Check your email", "A verification email has been sent to your new email address. Once you verify your email, the change will be applied.", MessageBoxButtons.Ok);
        }
    }

    private async Task UpdateNameAsync()
    {
        var result = await UseCases.UpdateUserNameAsync(ProfileModel);

        await MfaModal.HideAsync();

        if (result.Errors.Any())
        {
            Validation.SetErrors(result.Errors);
            ProfileModel.NewName = ProfileModel.CurrentName;
        }
        else
        {
            ProfileModel.CurrentName = ProfileModel.NewName;
            UserState.UpdateUserName(ProfileModel.NewName);
            await MessageBox.ShowAsync("Success", "Hi " + ProfileModel.NewName + ", your name has been updated successfully.", MessageBoxButtons.Ok);
        }
    }

    private async Task UpdatePasswordAsync()
    {
        var result = await UseCases.UpdateUserPasswordAsync(PasswordModel);

        PasswordModel.CurrentPassword = string.Empty;
        PasswordModel.NewPassword = string.Empty;
        PasswordModel.ConfirmPassword = string.Empty;

        await MfaModal.HideAsync();

        if (result.Errors.Any())
        {
            Validation.SetErrors(result.Errors);
        }
        else
        {
            await MessageBox.ShowAsync("Success", "Your password has been updated successfully.", MessageBoxButtons.Ok);
        }
    }

    private async Task DeleteUserAsync()
    {
        var confirmed = await Confirmation.Show("Deleting your account is permanent and cannot be undone. Your data will be permanently removed.");

        if (confirmed)
        {
            var result = await UseCases.DeleteUserAsync();

            if (!result.IsSuccess)
            {
                Validation.SetErrors(result.Errors);
            }
            else
            {
                await MessageBox.ShowAsync("Success", "Your account has been deleted successfully.", MessageBoxButtons.Ok);
                Navigation.NavigateTo(Pages.Login, true);
            }
        }
    }
}