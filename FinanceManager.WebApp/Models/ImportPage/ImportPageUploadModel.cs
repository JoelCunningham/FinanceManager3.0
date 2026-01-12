using FinanceManager.Application.DTOs;
using FinanceManager.WebApp.Enums;

namespace FinanceManager.WebApp.Models.ImportPage
{
    public class ImportPageUploadModel()
    {
        public ParserInfo? SelectedBank { get; set; }
        public string? FileErrorMessage { get; set; }
        public UploadValidationState FileValidationState { get; set; } = UploadValidationState.None;

        public void SetSuccess()
        {
            FileErrorMessage = null;
            FileValidationState = UploadValidationState.Valid;
        }

        public void SetError(Exception exception)
        {
            FileErrorMessage = GetErrorMessage(exception);
            FileValidationState = UploadValidationState.Invalid;
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
}