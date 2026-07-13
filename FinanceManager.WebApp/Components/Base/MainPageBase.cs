namespace FinanceManager.WebApp.Components.Base;

using FinanceManager.Application.Common;
using FinanceManager.Application.UseCases;
using FinanceManager.WebApp.Components.Layout;
using FinanceManager.WebApp.Models;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

[Authorize]
[Layout(typeof(MainLayout))]
public partial class MainPageBase : ComponentBase
{
    [Inject] public UseCases UseCases { get; set; } = default!;
    [Inject] public Preferences Preferences { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;

    [Inject] private IHxMessengerService Messenger { get; set; } = default!;

    [Parameter] public string? ActiveTabId { get; set; }

    public ValidationModel Validation { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;
    }
}
