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
        public ImportPageCategoriseModel CategoriseModel { get; set; } = new ImportPageCategoriseModel([]);

        public static ImportStage InitalStage { get; set; } = ImportStage.FileUpload;
        public override ImportStage MaxStageReached { get; set; } = InitalStage;
        protected override ImportStage[] Stages { get; } =
        [
            ImportStage.FileUpload,
            ImportStage.ConfigureTransfers,
            ImportStage.CategoriseTransactions,
            ImportStage.Complete
        ];

        public bool HasProgress => MaxStageReached != InitalStage;

        public void Reset()
        {
            ImportedTransactions = null;
            
            BankModel = new ImportPageUploadModel();
            TransfersModel = new ImportPageTransfersModel([]);
            CategoriseModel = new ImportPageCategoriseModel([]);

            MaxStageReached = ImportStage.FileUpload;
        }

        public void BankChanged(ParserInfo bank)
        {
            Reset();
            BankModel.SelectedBank = bank;
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
        
        public void TransfersSubmitted()
        {
            if (ImportedTransactions is null) return;

            var transactions = ImportedTransactions.Except(TransfersModel.AcceptedTransfers).ToList();
            
            foreach (var transfer in TransfersModel.RejectedTransfers)
            {
                var rejectedTransfer = transactions.FirstOrDefault(t => t.BankRecord.Id == transfer.BankRecord.Id);
                rejectedTransfer?.UnsetTransfer();
            }

            CategoriseModel = new ImportPageCategoriseModel(transactions);
        }

        public override bool CanIncrementStage => CurrentStage switch
        {
            ImportStage.FileUpload => BankModel.FileValidationState == UploadValidationState.Valid,
            ImportStage.ConfigureTransfers => TransfersModel.DetectedTransfers.Count == 0,
            ImportStage.CategoriseTransactions => false,
            ImportStage.Complete => false,
            _ => false
        };
    }
}