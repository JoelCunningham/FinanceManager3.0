namespace FinanceManager.WebApp.Components.Base;

using FinanceManager.Application.Common;
using FinanceManager.WebApp.Components.Layout;
using FinanceManager.WebApp.Models;
using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

[Authorize]
[Layout(typeof(MainLayout))]
public partial class MainPageBase : ComponentBase
{
    [Inject] public Preferences Preferences { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;

    [Inject] public IHxMessengerService Messenger { get; set; } = default!;
    [Inject] public IHxMessageBoxService MessageBox { get; set; } = default!;

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    [Parameter] public string? ActiveTabId { get; set; }

    public ValidationModel Validation { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;
    }
}
