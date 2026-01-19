using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Utilities
{
    public static class TypeConverter
    {
        public static Transaction BankRecordToTransaction(BankRecord record)
        {
            return new Transaction
            {
                Id = Guid.NewGuid(),
                RecordId = record.Id,
                Date = record.Date,
                Amount = record.Amount,
                Description = record.Description,
                CategoryId = null
            };
        }

        public static List<Transfer> BankRecordsToTransfers(IEnumerable<BankRecord> records)
        {
            List<Transfer> transfers = [];
            var transferTransactions = records.Where(t => t.IsInternalTransfer).ToList();

            foreach (var transfer in transferTransactions)
            {
                if (transfer.Amount < 0) continue;

                var match = TransferUtilities.FindTransferMatch(transfer, transferTransactions);

                if (match != null)
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
                ToRecordId = to.Id,
                Amount = from.Amount,
                Date = from.Date,
                Description = from.Description + " / " + to.Description,
                IsUserCreated = false
            };
        }

        public static IEnumerable<ParserViewData> ParsersToViewData(IEnumerable<ITransactionFileParser> parsers)
        {
            return [..parsers
                .GroupBy(parser => parser.GetBankName(), StringComparer.OrdinalIgnoreCase)
                .Select(group => new ParserViewData
                {
                    BankName = group.Key,
                    SupportedExtensions = [..group
                        .SelectMany(p => p.GetFileExtensions())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                    ]
                })
                .OrderBy(p => p.BankName)
            ];
        }

        public static TransferViewData TransferToViewData(Transfer transfer, BankRecord fromRecord, BankRecord toRecord)
        {
            return new TransferViewData
            {
                EntityId = transfer.Id,
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
            };
        }

        public static Transaction TransferToTransaction(Transfer transfer, bool useFromRecord)
        {
            var descriptionParts = transfer.Description.Split(" / ", 2);
            var description = useFromRecord ? descriptionParts[0] : descriptionParts[1];

            return new Transaction
            {
                Id = Guid.NewGuid(),
                RecordId = useFromRecord ? transfer.FromRecordId : transfer.ToRecordId,
                Date = transfer.Date,
                Amount = transfer.Amount,
                Description = description,
                CategoryId = null
            };
        }
    }
}
