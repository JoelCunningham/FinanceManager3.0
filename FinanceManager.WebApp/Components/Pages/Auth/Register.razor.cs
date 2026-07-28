namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Models;
using FinanceManager.WebApp.Components.Base;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;

[Route(Pages.Register)]
public partial class Register : AuthPageBase
{
    private RegisterUserModel Model { get; set; } = new();
    
    private string? Message { get; set; }
    private bool Success { get; set; }

    protected override void OnInitialized()
    {
        SetSidebar();
    }

    private void SetSidebar()
    {
        Layout?.UpdateSidebar(
            "Get started,",
            "in seconds.",
            "Create your free account and start understanding your finances today.",
            BootstrapIcon.PersonAdd);
    }

    private async Task HandleRegister()
    {
        Model.Origin = Navigation.BaseUri.TrimEnd('/');
        var result = await UseCases.RegisterUserAsync(Model);

        Success = result.IsSuccess;
        Message = result.Errors.FirstOrDefault()?.Message;
    }
}
