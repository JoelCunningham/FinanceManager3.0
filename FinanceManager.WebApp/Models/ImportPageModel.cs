namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.DTOs;
using FinanceManager.WebApp.Models.Base;

public class ImportPageModel
{
    public ValidationModel UploadValidation { get; set; } = new();
    public ValidationModel ImportValidation { get; set; } = new();

    public ParserSummary? SelectedBank { get; set; }
    public IReadOnlyList<ParsedTransaction>? ImportedTransactions { get; set; }
    public string SupportedExtensions => GetSupportedExtensions();

    public bool IsPreviewOpen { get; set; } = false;
    public bool IsSuccessOpen { get; set; } = false;

    public void Reset()
    {
        SelectedBank = null;
        ImportedTransactions = null;
        UploadValidation.ClearErrors();
        IsPreviewOpen = false;
        IsSuccessOpen = false;
    }

    public void UploadSuccess(IEnumerable<ParsedTransaction> records)
    {
        ImportedTransactions = [.. records];
        UploadValidation.SetSucess();
        IsPreviewOpen = false;
    }

    public void UploadError(Exception exception)
    {
        ImportedTransactions = null;
        UploadValidation.SetError(GetUploadErrorMessage(exception));
        IsPreviewOpen = false;
    }

    public void ImportSuccess()
    {
        ImportValidation.SetSucess();
        IsSuccessOpen = true;
    }

    public void ImportError()
    {
        ImportValidation.SetError("An unexpected error occurred. Please try again.");
        IsSuccessOpen = false;
    }

    public void NoTransactionsFound()
    {
        ImportedTransactions = null;
        UploadValidation.SetError("No new transactions were found in the uploaded file.");
        IsPreviewOpen = false;
    }

    public void BankChanged(ParserSummary bank)
    {
        Reset();
        SelectedBank = bank;
    }

    public void ShowPreview()
    {
        IsPreviewOpen = true;
    }

    public void ClosePreview()
    {
        IsPreviewOpen = false;
    }

    public void CloseSuccess()
    {
        Reset();
    }

    private string GetUploadErrorMessage(Exception exception)
    {
        if (SelectedBank is null) return "Please select a bank first.";

        return exception switch
        {
            IOException => "The file you uploaded is too large. Please upload a file below 512KB.",
            KeyNotFoundException => "The type of the file you uploaded is not supported. Please upload a file of type: " + string.Join(", ", SelectedBank.SupportedExtensions),
            _ => "Unable to import transactions from this file. Please check it is correct."
        };
    }

    private string GetSupportedExtensions()
    {
        if (SelectedBank is null) return string.Empty;
        return string.Join(", ", SelectedBank.SupportedExtensions);
    }
}