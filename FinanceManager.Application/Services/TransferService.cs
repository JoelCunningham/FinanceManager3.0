using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

namespace FinanceManager.Application.Services
{
    public class TransferService(
        ITransferRepository _transferRepository,
        IBankRecordRepository _bankRecordRepository
    )
    {
        public async Task<IEnumerable<TransferViewData>> GetAllAsync()
        {
            var result = new List<TransferViewData>();
            var transfers = await _transferRepository.GetAllAsync();

            var transferRecordIds = transfers
                .SelectMany(t => new[] { t.ToRecordId, t.FromRecordId })
                .Distinct()
                .ToList();

            var records = await _bankRecordRepository.GetByIdsAsync(transferRecordIds);
            var recordLookup = records.ToDictionary(r => r.Id);

            foreach (var transfer in transfers)
            {
                if (!recordLookup.TryGetValue(transfer.ToRecordId, out var toRecord) ||
                    !recordLookup.TryGetValue(transfer.FromRecordId, out var fromRecord))
                    continue;

                result.Add(new TransferViewData
                {
                    Amount = transfer.Amount,
                    Date = transfer.Date,
                    Description = transfer.Description,
                    IsUserCreated = transfer.IsUserCreated,
                    From = new Transferable
                    {
                        Bank = fromRecord.Bank,
                        Account = fromRecord.AccountNumber,
                    },
                    To = new Transferable
                    {
                        Bank = toRecord.Bank,
                        Account = toRecord.AccountNumber,
                    }
                });
            }

            return result;
        }
    }
}
