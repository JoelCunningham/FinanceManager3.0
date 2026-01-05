using FinanceManager.Application.DTOs;
using FinanceManager.WebApp.Enums;
using FinanceManager.WebApp.Models.Base;
using FinanceManager.WebApp.Models.ImportPage;

namespace FinanceManager.WebApp.Models
{
    public class ImportPageModel : NavigatableModel<ImportStage>
    {
        public IReadOnlyList<ImportedTransaction>? ImportedTransactions { get; set; }

        public ImportPageBankModel BankModel { get; set; } = new ImportPageBankModel();
        public ImportPageTransfersModel TransfersModel { get; set; } = new ImportPageTransfersModel([]);

        public override ImportStage MaxStageReached { get; set; } = ImportStage.FileUpload;
        protected override ImportStage[] Stages { get; } =
        [
            ImportStage.FileUpload,
            ImportStage.ConfigureTransfers,
            ImportStage.ConfigureReimbursements,
            ImportStage.CategorizeTransactions,
            ImportStage.Complete
        ];

        public void Reset()
        {
            ImportedTransactions = null;
            
            BankModel = new ImportPageBankModel();
            TransfersModel = new ImportPageTransfersModel([]);

            MaxStageReached = ImportStage.FileUpload;
        }

        public void ImportSuccess(IEnumerable<ImportedTransaction> records)
        {
            ImportedTransactions = records.ToList();
            TransfersModel = new ImportPageTransfersModel(ImportedTransactions);
            BankModel.SetSuccess();
        }

        public void ImportError(Exception exception)
        {
            ImportedTransactions = null;
            TransfersModel = new ImportPageTransfersModel([]);
            BankModel.SetError(exception);
        }

        public override bool CanIncrementStage => CurrentStage switch
        {
            ImportStage.FileUpload => BankModel.FileValidationState == FileValidationState.Valid,
            ImportStage.ConfigureTransfers => TransfersModel.DetectedTransfers.Count == 0,
            ImportStage.ConfigureReimbursements => true,
            ImportStage.CategorizeTransactions => true,
            ImportStage.Complete => false,
            _ => false
        };
    }
}