namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.Constants;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Import;
using FinanceManager.WebApp.Components.Features.Import;
using FinanceManager.WebApp.Utilities;
using Microsoft.AspNetCore.Components.Forms;

public partial class Import : PageBase
{
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
        AvailableParsers = UseCases.GetParsers().Parsers;
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

        var parseResult = await UseCases.ParseFileAsync(stream, bankName, fileExtension);

        if (parseResult.IsSuccess && parseResult.Transactions is not null)
        {
            ImportedTransactions = [.. parseResult.Transactions];
            Validation.SetSuccess($"{parseResult.Transactions.Count} new {LanguageUtilities.Pluralise("transactions", parseResult.Transactions.Count)} found.");
        }
        else
        {
            ImportedTransactions = null;
            foreach (var error in parseResult.Errors)
            {
                Validation.SetError(error.Message);
            }
        }
    }

    protected async Task ImportTransactions()
    {
        if (ImportedTransactions is null) return;

        var saveResult = await UseCases.SaveImportAsync(ImportedTransactions);

        if (saveResult is not null)
        {
            Validation.SetSuccess($"Imported {saveResult.RecordsSaved} records ({saveResult.TransactionsSaved} transactions, {saveResult.TransfersSaved} transfers).", true);
            await SuccessModal.ShowAsync();
        }
        else
        {
            Validation.SetError(ErrorMessages.DefaultErrorMessage);
        }
    }
}