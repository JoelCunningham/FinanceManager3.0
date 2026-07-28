using FinanceManager.Application;
using FinanceManager.Application.Configuration;
using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Infrastructure;
using FinanceManager.Infrastructure.Data;
using FinanceManager.Infrastructure.Identity;
using FinanceManager.WebApp.Components;
using FinanceManager.WebApp.Components.Pages.Auth;
using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

//----- Create builder -----//
var builder = WebApplication.CreateBuilder(args);

// Config
builder.Services.AddOptions<EmailConfig>().Bind(builder.Configuration.GetSection(EmailConfig.SectionName));
builder.Services.AddOptions<TokenConfig>().Bind(builder.Configuration.GetSection(TokenConfig.SectionName));
builder.Services.AddOptions<CookieConfig>().Bind(builder.Configuration.GetSection(CookieConfig.SectionName));
builder.Services.AddOptions<IdentityConfig>().Bind(builder.Configuration.GetSection(IdentityConfig.SectionName));

// Data protection
var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection().SetApplicationName("FinanceManager.WebApp").PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

// Blazor
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHxServices().AddHxMessenger().AddHxMessageBoxHost();

// Database
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Missing connection string. Set ConnectionStrings__Default.");
}

// Dependency injection
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<FinanceManagerDbContext>().AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(options =>
{
    var config = builder.Configuration.GetSection(IdentityConfig.SectionName).Get<IdentityConfig>()!;

    options.SignIn.RequireConfirmedAccount = config.RequireConfirmedAccount;

    options.Password.RequireDigit = config.Password.RequireDigit;
    options.Password.RequireUppercase = config.Password.RequireUppercase;
    options.Password.RequireLowercase = config.Password.RequireLowercase;
    options.Password.RequireNonAlphanumeric = config.Password.RequireNonAlphanumeric;
    options.Password.RequiredLength = config.Password.RequiredLength;

    options.User.RequireUniqueEmail = config.User.RequireUniqueEmail;

    options.Lockout.MaxFailedAccessAttempts = config.Lockout.MaxFailedAccessAttempts;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(config.Lockout.DefaultLockoutTimeSpan);
});

// Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    var config = builder.Configuration.GetSection(CookieConfig.SectionName).Get<CookieConfig>()!;

    options.LoginPath = config.LoginPath;
    options.LogoutPath = config.LogoutPath;

    options.Cookie.HttpOnly = config.HttpOnly;
    options.ExpireTimeSpan = TimeSpan.FromDays(config.ExpireDays);
    options.SlidingExpiration = config.SlidingExpiration;
});

// Token
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    var config = builder.Configuration.GetSection(TokenConfig.SectionName).Get<TokenConfig>()!;

    options.TokenLifespan = TimeSpan.FromHours(config.TokenLifespan);
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

//----- Create application -----//
var app = builder.Build();

// Migrations
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceManagerDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(Pages.Error, createScopeForErrors: true);
    app.UseHsts();
}

// Redirection
app.UseStatusCodePagesWithReExecute(Pages.NotFound, createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Mapping
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapAuthenticationEndpoints();

//----- Run application -----//
app.Run();