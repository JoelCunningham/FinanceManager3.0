using FinanceManager.Application.DTOs;
using FinanceManager.WebApp.Enums;
using FinanceManager.WebApp.Models.Base;

namespace FinanceManager.WebApp.Models.ImportPage
{
    public class ImportPageModel : NavigatableModel<ImportStage>
    {
        public IReadOnlyList<ImportedTransaction>? ImportedTransactions { get; set; }

        public ImportPageUploadModel BankModel { get; set; } = new ImportPageUploadModel();
        public ImportPageTransfersModel TransfersModel { get; set; } = new ImportPageTransfersModel([]);
        public ImportPageReimbursementsModel ReimbursementsModel { get; set; } = new ImportPageReimbursementsModel([]);

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
            
            BankModel = new ImportPageUploadModel();
            TransfersModel = new ImportPageTransfersModel([]);
            ReimbursementsModel = new ImportPageReimbursementsModel([]);

            MaxStageReached = ImportStage.FileUpload;
        }

        public void ImportSuccess(IEnumerable<ImportedTransaction> records)
        {
            ImportedTransactions = records.ToList();
            TransfersModel = new ImportPageTransfersModel(ImportedTransactions);
            ReimbursementsModel = new ImportPageReimbursementsModel(ImportedTransactions);
            BankModel.SetSuccess();
        }

        public void ImportError(Exception exception)
        {
            ImportedTransactions = null;
            TransfersModel = new ImportPageTransfersModel([]);
            ReimbursementsModel = new ImportPageReimbursementsModel([]);
            BankModel.SetError(exception);
        }

        public override bool CanIncrementStage => CurrentStage switch
        {
            ImportStage.FileUpload => BankModel.FileValidationState == UploadValidationState.Valid,
            ImportStage.ConfigureTransfers => TransfersModel.DetectedTransfers.Count == 0,
            ImportStage.ConfigureReimbursements => false,
            ImportStage.CategorizeTransactions => false,
            ImportStage.Complete => false,
            _ => false
        };
    }
}