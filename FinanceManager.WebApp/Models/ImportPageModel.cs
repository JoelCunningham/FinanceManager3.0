using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;

namespace FinanceManager.WebApp.Models
{
    public sealed class ImportPageModel
    {
        public ParserInfo? SelectedBank { get; set; }
        public IReadOnlyList<BankRecord>? ImportedTransactions { get; set; }
        public string? ImportErrorMessage { get; set; }
        public FileValidationState FileValidation { get; private set; } = FileValidationState.None;

        public void Reset()
        {
            ImportErrorMessage = null;
            ImportedTransactions = null;
            FileValidation = FileValidationState.None;
        }

        public void Success(IEnumerable<BankRecord> records)
        {
            ImportedTransactions = records.ToList();
            ImportErrorMessage = null;
            FileValidation = FileValidationState.Valid;
        }

        public void Error(Exception exception)
        {
            ImportedTransactions = null;
            ImportErrorMessage = GetErrorMessage(exception);
            FileValidation = FileValidationState.Invalid;
        }

        private string GetErrorMessage(Exception exception)
        {
            if (SelectedBank is null)
            {
                return "Please select a bank first.";
            }
            return exception switch
            {
                IOException => "The file you uploaded is too large. Please upload a file below 512KB.",
                KeyNotFoundException => "The type of the file you uploaded is not supported. Please upload a file of type: " + string.Join(", ", SelectedBank.SupportedExtensions),
                _ => "Unable to import transactions from this file. Please check it is correct."
            };
        }
    }

    public enum FileValidationState
    {
        None,
        Valid,
        Invalid
    }
}