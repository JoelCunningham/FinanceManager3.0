using FinanceManager.Application.DTOs;
using FinanceManager.WebApp.Enums;
using FinanceManager.WebApp.Models.Base;

namespace FinanceManager.WebApp.Models
{
    public class ImportPageModel : NavigatableModel<ImportStage>
    {
        public ParserInfo? SelectedBank { get; set; }

        public string? ImportErrorMessage { get; set; }
        public FileValidationState FileValidation { get; set; } = FileValidationState.None;

        public IReadOnlyList<ImportedTransaction>? ImportedTransactions { get; set; }
        public List<ImportedTransaction> CustomisedTransactions { get; set; } = [];

        public override ImportStage MaxStageReached { get; set; } = ImportStage.FileUpload;
        protected override ImportStage[] Stages { get; } =
        [
            ImportStage.FileUpload,
            ImportStage.ConfigureTransactions,
            ImportStage.ConfigureReimbursements,
            ImportStage.CategorizeTransactions,
            ImportStage.Complete
        ];

        public void Reset()
        {
            ImportErrorMessage = null;
            ImportedTransactions = null;
            FileValidation = FileValidationState.None;
            MaxStageReached = ImportStage.FileUpload;
        }

        public void ImportSuccess(IEnumerable<ImportedTransaction> records)
        {
            ImportedTransactions = records.ToList();
            ImportErrorMessage = null;
            FileValidation = FileValidationState.Valid;
        }

        public void ImportError(Exception exception)
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

        public override bool CanIncrementStage => CurrentStage switch
        {
            ImportStage.FileUpload => FileValidation == FileValidationState.Valid,
            ImportStage.ConfigureTransactions => true,
            ImportStage.ConfigureReimbursements => true,
            ImportStage.CategorizeTransactions => true,
            ImportStage.Complete => false,
            _ => false
        };
    }
}