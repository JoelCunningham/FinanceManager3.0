using CsvHelper.Configuration;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Parsers.Base;

namespace FinanceManager.Infrastructure.Parsers
{
    public class WestpacTransactionFileParser : BaseTransactionFileParser
    {
        public override string GetDisplayName() => "Westpac";
        public override IEnumerable<string> GetSupportedFileExtensions() => [".csv"];
        protected override ClassMap<BankRecord> GetClassMap() => new WestpacBankRecordMap();

        private sealed class WestpacBankRecordMap : ClassMap<BankRecord>
        {
            public WestpacBankRecordMap()
            {
                Map(m => m.AccountNumber).Name("Bank Account");
                Map(m => m.Date).Name("Date");
                Map(m => m.Narrative).Name("Narrative");
                Map(m => m.DebitAmount).Name("Debit Amount");
                Map(m => m.CreditAmount).Name("Credit Amount");
                Map(m => m.Category).Name("Categories");
            }
        }
    }
}