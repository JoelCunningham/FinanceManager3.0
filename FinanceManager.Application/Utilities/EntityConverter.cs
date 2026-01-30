using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Utilities
{
    public static class EntityConverter
    {
        public static Transaction BankRecordToTransaction(BankRecord record)
        {
            return new Transaction
            {
                Id = Guid.NewGuid(),
                RecordId = record.Id,
                Record = record,
                Date = record.Date,
                Amount = record.Amount,
                Description = record.Description,
                CategoryId = null
            };
        }

        public static Reimbursement TransactionToReimbursement(Transaction transaction)
        {
            return new Reimbursement
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                Transaction = transaction,
                RecordId = transaction.RecordId,
                Record = transaction.Record,
                Date = transaction.Date,
                Amount = transaction.Amount,
                Description = transaction.Description
            };
        }

        public static List<Transfer> BankRecordsToTransfers(IEnumerable<BankRecord> records)
        {
            List<Transfer> transfers = [];

            foreach (var transfer in records)
            {
                if (transfer.Amount < 0) continue;

                var match = FindTransferMatch(transfer, records);

                if (match is not null)
                {
                    transfers.Add(BankRecordPairToTransfer(transfer, match));
                }
            }

            return transfers;
        }

        public static Transfer BankRecordPairToTransfer(BankRecord from, BankRecord to)
        {
            return new Transfer
            {
                Id = Guid.NewGuid(),
                FromRecordId = from.Id,
                FromRecord = from,
                ToRecordId = to.Id,
                ToRecord = to,
                Amount = from.Amount,
                Date = from.Date,
                Description = from.Description + " / " + to.Description,
                IsUserCreated = false
            };
        }

        private static BankRecord? FindTransferMatch(BankRecord transfer, IEnumerable<BankRecord> candidates)
        {
            return candidates.FirstOrDefault(t =>
                t != transfer &&
                t.Amount == -transfer.Amount &&
                t.Date.Date == transfer.Date.Date);
        }
    }
}
