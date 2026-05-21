namespace FinanceManager.WebApp.Components.Base;

using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;

public partial class ModalBase : ComponentBase
{
    protected  HxModal _modal = default!;

    public Task ShowAsync() => _modal?.ShowAsync() ?? Task.CompletedTask;
    public Task HideAsync() => _modal?.HideAsync() ?? Task.CompletedTask;
}