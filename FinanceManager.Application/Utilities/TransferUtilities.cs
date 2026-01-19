using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Utilities
{
    public static class TransferUtilities
    {
        public static BankRecord? FindTransferMatch(BankRecord transfer, IEnumerable<BankRecord> candidates)
        {
            return candidates.FirstOrDefault(t =>
                t != transfer &&
                t.Amount == -transfer.Amount &&
                t.Date.Date == transfer.Date.Date);
        }
    }
}
