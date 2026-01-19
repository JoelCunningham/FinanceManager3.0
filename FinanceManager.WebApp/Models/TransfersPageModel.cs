using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models
{
    public class TransfersPageModel()
    {
        public List<TransferViewData> Transfers { get; set; } = [];

        public TransferViewData? ErrorTransfer { get; set; }
        public string? RemoveErrorMessage { get; set; }

        public void Reset()
        {
            ErrorTransfer = null;
            RemoveErrorMessage = null;
        }

        public void RemoveError(TransferViewData transfer)
        {
            ErrorTransfer = transfer;
            RemoveErrorMessage = "Could not remove transfer. Please try again.";
        }
    }
}