namespace FinanceManager.WebApp.Components.Base;

using FinanceManager.Application.UseCases;
using FinanceManager.WebApp.Models;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Components;

public partial class PageBase : ComponentBase
{
    [Inject] public UseCases UseCases { get; set; } = default!;
    [Inject] public IHxMessengerService Messenger { get; set; } = default!;
    
    public ValidationModel Validation { get; set; } = new();
}
