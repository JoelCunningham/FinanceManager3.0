namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.Components;

[Route(Pages.Redirect)]
public partial class Redirect : AuthPageBase
{
    [Parameter, EditorRequired] public required string Destination { get; set; }

    protected override void OnInitialized()
    {
        var parameters = new Dictionary<string, string?>
        {
            [Parameters.ReturnPath] = Navigation.ToBaseRelativePath(Navigation.Uri)
        };

        var url = Navigator.CreateUrl(Destination, parameters);

        Navigation.NavigateTo(url);
    }
}