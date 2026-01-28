using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;

namespace FinanceManager.Application.Services
{
    public class TransferService(
        ITransferRepository _transferRepository,
        IBankRecordRepository _bankRecordRepository,
        ITransactionRepository _transactionRepository,
        IUnitOfWork _unitOfWork
    )
    {
        public async Task<IEnumerable<TransferSummary>> GetAllAsync()
        {
            var result = new List<TransferSummary>();
            var transfers = await _transferRepository.GetAllAsync();

            var transferRecordIds = transfers.SelectMany(t => new[] { t.ToRecordId, t.FromRecordId });

            var records = await _bankRecordRepository.GetByIdsAsync(transferRecordIds);
            var recordsLookup = records.ToDictionary(r => r.Id);

            foreach (var transfer in transfers)
            {
                if (!recordsLookup.TryGetValue(transfer.ToRecordId, out var toRecord) ||
                    !recordsLookup.TryGetValue(transfer.FromRecordId, out var fromRecord))
                    continue;

                result.Add(TransferSummary.FromTransfer(transfer, fromRecord, toRecord));
            }

            return result;
        }

        public async Task RemoveTransfer(TransferSummary transferViewData)
        {
            await using var transaction = _unitOfWork.BeginTransaction();

            try
            {
                var transfer = await _transferRepository.GetByIdAsync(transferViewData.EntityId);

                var fromRecord = await _bankRecordRepository.GetByIdAsync(transfer.FromRecordId);
                var toRecord = await _bankRecordRepository.GetByIdAsync(transfer.ToRecordId);

                await _transferRepository.RemoveAsync(transfer.Id);

                var fromTransaction = EntityConverter.BankRecordToTransaction(fromRecord);
                var toTransaction = EntityConverter.BankRecordToTransaction(toRecord);

                await _transactionRepository.SaveAsync([fromTransaction, toTransaction]);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
