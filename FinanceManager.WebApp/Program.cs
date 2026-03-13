using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Services;
using FinanceManager.Infrastructure.Parsers;
using FinanceManager.Infrastructure.Repositories.InMemory;
using FinanceManager.WebApp.Components;
using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHxServices().AddHxMessenger().AddHxMessageBoxHost();

// Repositories (TODO change to scoped when using a database) 
builder.Services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
builder.Services.AddSingleton<ITransferRepository, TransferRepository>();
builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();
builder.Services.AddSingleton<IBankRecordRepository, BankRecordRepository>();
builder.Services.AddSingleton<IReimbursementRepository, ReimbursementRepository>();
builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<IMachineLearningRepository, MachineLearningRepository>();
builder.Services.AddSingleton<IBudgetEntryRepository, BudgetEntryRepository>();
builder.Services.AddSingleton<IBudgetPeriodRepository, BudgetPeriodRepository>();

// Parsers
builder.Services.AddSingleton<ITransactionFileParser, WestpacTransactionFileParser>();
builder.Services.AddSingleton<ITransactionFileParser, VanguardTransactionFileParser>();

// Services
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TransferService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<ImportService>();
builder.Services.AddScoped<CategorisationService>();
builder.Services.AddScoped<BudgetService>();
builder.Services.AddSingleton<ParserService>();

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
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();