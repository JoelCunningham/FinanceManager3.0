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

            foreach (var transfer in records)
            {
                if (transfer.Amount < 0) continue;

                var match = TransferUtilities.FindTransferMatch(transfer, records);

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
                ToRecordId = to.Id,
                Amount = from.Amount,
                Date = from.Date,
                Description = from.Description + " / " + to.Description,
                IsUserCreated = false
            };
        }

        public static UnreviewedTransactions BankRecordToUncategorisedViewData(BankRecord record)
        {
            return new UnreviewedTransactions
            {
                Record = record,
                Transactions = [new()
                {
                    Description = record.Description,
                    Amount = record.Amount,
                    Date = record.Date,
                }]
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

        public static TransferSummary TransferToViewData(Transfer transfer, BankRecord fromRecord, BankRecord toRecord)
        {
            return new TransferSummary
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
    }
}
