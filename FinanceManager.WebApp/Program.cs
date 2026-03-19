using FinanceManager.Application;
using FinanceManager.Infrastructure;
using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHxServices().AddHxMessenger().AddHxMessageBoxHost();

builder.Services.AddInfrastructure();
builder.Services.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<FinanceManager.WebApp.Components.App>().AddInteractiveServerRenderMode();

app.Run();