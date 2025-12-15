using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Services;
using FinanceManager.Infrastructure.Parsers;
using FinanceManager.Infrastructure.Repositories.InMemory;
using FinanceManager.WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

// Repositories
builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();
builder.Services.AddSingleton<IBankRecordRepository, BankRecordRepository>();

// Parsers
builder.Services.AddSingleton<ITransactionFileParser, WestpacTransactionFileParser>();
builder.Services.AddSingleton<ITransactionFileParser, VanguardTransactionFileParser>();

// Services
builder.Services.AddTransient<TransactionService>();
builder.Services.AddTransient<ImportService>();
builder.Services.AddTransient<TransactionFileParserFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

app.Run();
