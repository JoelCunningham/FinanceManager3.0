namespace FinanceManager.WebApp.Components.Base;

using FinanceManager.WebApp.Components.Layout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

[AllowAnonymous]
[Layout(typeof(ErrorLayout))]
public partial class ErrorPageBase : MainPageBase
{
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
}