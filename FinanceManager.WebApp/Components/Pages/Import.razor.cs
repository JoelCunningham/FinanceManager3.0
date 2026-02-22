using FinanceManager.Application.DTOs;
using FinanceManager.Application.Services;
using FinanceManager.WebApp.Components.Features.Import;
using FinanceManager.WebApp.Models.Base;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FinanceManager.WebApp.Components.Pages;

public partial class Import : ComponentBase
{
    [Inject] public ImportService ImportService { get; set; } = default!;
    [Inject] public ParserService ParserService { get; set; } = default!;
    [Inject] public IHxMessengerService Messenger { get; set; } = default!;

    public ValidationModel Validation { get; set; } = new();

    public IReadOnlyList<ParserSummary> AvailableParsers { get; set; } = [];
    public ParserSummary? SelectedParser { get; set; }
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
        AvailableParsers = [.. ParserService.GetAvailableParsers()];
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
        try
        {
            await using var stream = file.OpenReadStream();
            var bankName = SelectedParser.BankName;
            var fileExtension = Path.GetExtension(file.Name);

            var results = (await ImportService.ImportAsync(stream, bankName, fileExtension)).ToList();
            if (results.Count == 0) throw new ArgumentNullException(nameof(file));

            ImportedTransactions = [.. results];
            Validation.SetSuccess($"{results.Count} new tranasctions found.", true);
        }
        catch (Exception exception)
        {
            ImportedTransactions = null;
            Validation.SetError(GetUploadErrorMessage(exception), true);
        }
    }

    protected async Task ImportTransactions()
    {
        if (ImportedTransactions is null) return;
        try
        {
            await ImportService.SaveAsync(ImportedTransactions);
            await SuccessModal.ShowAsync();
        }
        catch
        {
            Validation.SetError("An unexpected error occurred. Please try again.");
        }
    }

    private string GetUploadErrorMessage(Exception exception)
    {
        if (SelectedParser is null) return "Please select a bank first.";
        return exception switch
        {
            IOException => "The file you uploaded is too large. Please upload a file below 512KB.",
            ArgumentNullException => "No new transactions were found in the uploaded file.",
            KeyNotFoundException => "The type of the file you uploaded is not supported. Please upload a file of type: " + string.Join(", ", SelectedParser.SupportedExtensions),
            _ => "Unable to import transactions from this file. Please check it is correct."
        };
    }
}