namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

[Route(Pages.ConfirmEmail)]
public partial class ConfirmEmail : AuthPageBase
{
    private string? Message { get; set; }
    private bool Success { get; set; }

    private const string GenericError = "Your email confirmation link has expired. Please try again. You can use the same email address.";

    protected override async Task OnInitializedAsync()
    {
        Message = await AttemptConfirmEmail();
        Success = Message is null;
    }

    private async Task<string?> AttemptConfirmEmail()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        var email = query[Parameters.Email];
        var token = query[Parameters.Token];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return GenericError;
        }

        var user = await UserManager.FindByEmailAsync(Uri.UnescapeDataString(email));
        if (user == null)
        {
            return GenericError;
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        var result = await UserManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            return GenericError;
        }

        return null;
    }
}