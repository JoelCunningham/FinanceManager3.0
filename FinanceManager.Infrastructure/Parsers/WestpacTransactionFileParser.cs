using FinanceManager.Application.DTOs;
using FinanceManager.Infrastructure.Parsers.Base;

namespace FinanceManager.Infrastructure.Parsers
{
    public class WestpacTransactionFileParser : CsvTransactionFileParser<WestpacTransactionFile>
    {
        public override string GetBankName() => "Westpac";
        public override string GetDateFormat() => "dd/MM/yyyy";
        public override IEnumerable<string> GetFileExtensions() => [".csv"];
        public override IEnumerable<ParsedTransaction> StandardiseRecords(IEnumerable<WestpacTransactionFile> file)
        {
            foreach (var record in file)
            {
                var parsedTransaction = new ParsedTransaction
                {
                    Id = Guid.NewGuid(),
                    Bank = GetBankName(),
                    AccountNumber = record.BankAccount,
                    Date = record.Date,
                    Amount = (record.CreditAmount ?? 0) - (record.DebitAmount ?? 0),
                    Description = record.Narrative,
                    Type = record.Categories,
                    Reference = record.Serial,
                    IsInternalTransfer = record.Narrative.Contains(" TFR ")
                };
                yield return parsedTransaction;
            }
        }
    }

    public sealed class WestpacTransactionFile
    {
        public required string BankAccount { get; set; }
        public DateTime Date { get; set; }
        public required string Narrative { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal Balance { get; set; }
        public required string Categories { get; set; }
        public string? Serial { get; set; }
    }
}