using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models
{
    public class TransfersPageModel()
    {
        public List<TransferViewData> Transfers { get; set; } = [];
    }
}