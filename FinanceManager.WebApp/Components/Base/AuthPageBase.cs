namespace FinanceManager.WebApp.Components.Base;

using FinanceManager.WebApp.Components.Layout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

[AllowAnonymous]
[Layout(typeof(AuthLayout))]
public partial class AuthPageBase : MainPageBase
{
    [CascadingParameter] public AuthLayout? Layout { get; set; }
}