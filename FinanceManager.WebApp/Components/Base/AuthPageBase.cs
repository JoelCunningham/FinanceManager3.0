namespace FinanceManager.WebApp.Components.Base;

using FinanceManager.Infrastructure.Identity;
using FinanceManager.WebApp.Authentication;
using FinanceManager.WebApp.Components.Layout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

[AllowAnonymous]
[Layout(typeof(AuthLayout))]
public partial class AuthPageBase : MainPageBase
{
    [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
    [Inject] protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
    [Inject] protected AuthenticationService Authentication { get; set; } = default!;
}