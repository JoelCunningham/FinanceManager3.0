namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.WebApp.Components.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

[Route(Pages.Redirect)]
public partial class Redirect : AuthPageBase
{
    [Parameter, EditorRequired] public required string Destination { get; set; }

    protected override void OnInitialized()
    {
        var redirectLink = QueryHelpers.AddQueryString(Destination, Parameters.ReturnPath, Navigation.ToBaseRelativePath(Navigation.Uri));
        Navigation.NavigateTo(redirectLink);
    }
}