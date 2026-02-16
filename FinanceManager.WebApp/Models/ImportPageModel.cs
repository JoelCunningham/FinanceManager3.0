namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.DTOs;
using FinanceManager.WebApp.Models.Base;
using Google.GenAI.Types;
using Havit.Blazor.Components.Web.Bootstrap;

public class ImportPageModel
{
    public ValidationModel Validation { get; set; } = new();

    public IReadOnlyList<ParserSummary> AvailableParsers { get; set; } = [];
    public ParserSummary? SelectedParser { get; set; }
    public IReadOnlyList<ParsedTransaction>? ImportedTransactions { get; set; }

    public HxModal PreviewModal { get; set; } = new();
    public HxModal SuccessModal { get; set; } = new();

    public int? TransfersCount => ImportedTransactions?.Count(t => t.IsInternalTransfer);
    public string SupportedExtensions => SelectedParser is not null ? string.Join(", ", SelectedParser.SupportedExtensions) : string.Empty;

    public void Reset()
    {
        SelectedParser = null;
        ImportedTransactions = null;
        Validation.ClearValidation();
    }

    public void BankChanged()
    {
        ImportedTransactions = null;
        Validation.ClearValidation();
    }

    public string GetUploadErrorMessage(Exception exception)
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