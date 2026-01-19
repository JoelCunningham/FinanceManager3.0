using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;

namespace FinanceManager.Application.Services
{
    public class TransferService(
        ITransferRepository _transferRepository,
        IBankRecordRepository _bankRecordRepository,
        ITransactionRepository _transactionRepository
    )
    {
        public async Task<IEnumerable<TransferViewData>> GetAllAsync()
        {
            var result = new List<TransferViewData>();
            var transfers = await _transferRepository.GetAllAsync();

            var transferRecordIds = transfers.SelectMany(t => new[] { t.ToRecordId, t.FromRecordId });

            var records = await _bankRecordRepository.GetByIdsAsync(transferRecordIds);
            var recordsLookup = records.ToDictionary(r => r.Id);

            foreach (var transfer in transfers)
            {
                if (!recordsLookup.TryGetValue(transfer.ToRecordId, out var toRecord) ||
                    !recordsLookup.TryGetValue(transfer.FromRecordId, out var fromRecord))
                    continue;

                result.Add(TypeConverter.TransferToViewData(transfer, fromRecord, toRecord));
            }

            return result;
        }

        public async Task<bool> RemoveTransfer(TransferViewData transferViewData)
        {
            var success = false;
            
            var transfer = await _transferRepository.GetByIdAsync(transferViewData.EntityId);
           
            if (transfer == null) return success;

            success = await _transferRepository.RemoveAsync(transfer.Id);

            if (success)
            {
                var fromTransaction = TypeConverter.TransferToTransaction(transfer, true);
                var toTransaction = TypeConverter.TransferToTransaction(transfer, false);

                success = await _transactionRepository.SaveAsync([fromTransaction, toTransaction]);
            }

            return success;
        }
    }
}
