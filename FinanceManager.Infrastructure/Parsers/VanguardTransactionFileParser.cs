using FinanceManager.Application.DTOs;
using FinanceManager.Infrastructure.Parsers.Base;

namespace FinanceManager.Infrastructure.Parsers
{
    public class VanguardTransactionFileParser : CsvTransactionFileParser<VanguardTransactionFile>
    {
        public override string GetBankName() => "Vanguard";
        public override IEnumerable<string> GetFileExtensions() => [".csv"];
        public override IEnumerable<ParsedTransaction> StandardiseRecords(IEnumerable<VanguardTransactionFile> file)
        {
            foreach (var record in file)
            {
                var parsedTransaction = new ParsedTransaction
                {
                    Id = Guid.NewGuid(),
                    Bank = GetBankName(),
                    Date = record.Date,
                    Amount = record.Total,
                    Description = $"{record.ProductName} ({record.ProductId}) - {record.ProductType}{(record.Units is not null ? $" - {record.Units} units" : string.Empty)}",
                    Type = record.Type,
                    Reference = null,
                    IsInternalTransfer = false
                };
                yield return parsedTransaction;
            }
        }
    }

    public sealed class VanguardTransactionFile
    {
        public DateTime Date { get; set; }
        public required string Type { get; set; }
        public required string ProductType { get; set; }
        public required string ProductName { get; set; }
        public required string ProductId { get; set; }
        public int? Units { get; set; }
        public decimal Total { get; set; }
    }
}
