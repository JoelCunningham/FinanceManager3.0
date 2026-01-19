using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;
using FinanceManager.WebApp.Enums;

namespace FinanceManager.WebApp.Models
{
    public class ImportPageModel
    {
        public ParserInfo? SelectedBank { get; set; }
        public IReadOnlyList<BankRecord>? ImportedTransactions { get; set; }
        public string SupportedExtensions => GetSupportedExtensions();

        public string? UploadErrorMessage { get; set; }
        public string? ImportErrorMessage { get; set; }
        public FormValidationState UploadValidationState { get; set; } = FormValidationState.None;


        public bool IsPreviewOpen = false;

        public bool IsSuccessOpen = false;

        public void Reset()
        {
            SelectedBank = null;
            ImportedTransactions = null;
            UploadErrorMessage = null;
            UploadValidationState = FormValidationState.None;
            IsPreviewOpen = false;
            IsSuccessOpen = false;
        }

        public void UploadSuccess(IEnumerable<BankRecord> records)
        {
            ImportedTransactions = [.. records];
            UploadErrorMessage = null;
            UploadValidationState = FormValidationState.Valid;
            IsPreviewOpen = false;
        }

        public void UploadError(Exception exception)
        {
            ImportedTransactions = null;
            UploadErrorMessage = GetUploadErrorMessage(exception);
            UploadValidationState = FormValidationState.Invalid;
            IsPreviewOpen = false;
        }

        public void ImportSuccess()
        {
            ImportErrorMessage = null;
            IsSuccessOpen = true;
        }

        public void ImportError()
        {
            ImportErrorMessage = "An unexpected error occurred. Please try again.";
            IsSuccessOpen = false;
        }

        public void NoTransactionsFound()
        {
            ImportedTransactions = null;
            UploadErrorMessage = "No new transactions were found in the uploaded file.";
            UploadValidationState = FormValidationState.Invalid;
            IsPreviewOpen = false;
        }

        public void BankChanged(ParserInfo bank)
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
}