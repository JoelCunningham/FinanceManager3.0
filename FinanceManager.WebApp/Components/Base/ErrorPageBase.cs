namespace FinanceManager.WebApp.Components.Base;

using FinanceManager.WebApp.Components.Layout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

[AllowAnonymous]
[Layout(typeof(ErrorLayout))]
public partial class ErrorPageBase : MainPageBase
{

}