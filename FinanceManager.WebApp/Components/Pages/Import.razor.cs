namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Import;
using FinanceManager.WebApp.Components.Features.Import;
using FinanceManager.WebApp.Models;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

public partial class Import : ComponentBase
{
    [Inject] public ImportWorkflow ImportWorkflow { get; set; } = default!;
    [Inject] public IHxMessengerService Messenger { get; set; } = default!;

    public ValidationModel Validation { get; set; } = new();

    public IReadOnlyList<BankParser> AvailableParsers { get; set; } = [];
    public BankParser? SelectedParser { get; set; }
    public IReadOnlyList<ParsedTransaction>? ImportedTransactions { get; set; }

    public PreviewModal PreviewModal { get; set; } = new();
    public SuccessModal SuccessModal { get; set; } = new();

    public bool ShowBankSelection => AvailableParsers.Count > 1;
    public bool ShowFileUpload => SelectedParser is not null;
    public bool ShowActionButtons => ImportedTransactions is not null && ImportedTransactions.Count > 0;
    public string SupportedExtensions => SelectedParser is not null ? string.Join(", ", SelectedParser.SupportedExtensions) : string.Empty;

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;
        AvailableParsers = ImportWorkflow.GetParsers();
    }

    protected void Reset()
    {
        SelectedParser = null;
        ImportedTransactions = null;
        Validation.Clear();
    }

    protected void BankChanged()
    {
        ImportedTransactions = null;
        Validation.Clear();
    }

    protected async Task ParseFile(IBrowserFile file)
    {
        if (SelectedParser is null || file is null) return;

        await using var stream = file.OpenReadStream();
        var bankName = SelectedParser.BankName;
        var fileExtension = Path.GetExtension(file.Name);

        var parseResult = await ImportWorkflow.ParseFileAsync(stream, bankName, fileExtension);

        if (parseResult.IsSuccess)
        {
            ImportedTransactions = [.. parseResult.Transactions];
            Validation.SetSuccess($"{parseResult.Transactions.Count()} new tranasctions found.", true);
        }
        else
        {
            ImportedTransactions = null;
            Validation.SetError(parseResult.ErrorMessage!, true);
        }
    }

    protected async Task ImportTransactions()
    {
        if (ImportedTransactions is null) return;

        var saveResult = await ImportWorkflow.SaveImportAsync(ImportedTransactions);

        if (saveResult is not null)
        {
            Validation.SetSuccess($"Imported {saveResult.RecordsSaved} records ({saveResult.TransactionsSaved} transactions, {saveResult.TransfersSaved} transfers).", true);
            await SuccessModal.ShowAsync();
        }
        else
        {
            Validation.SetError("An unexpected error occurred. Please try again.");
        }
    }
}