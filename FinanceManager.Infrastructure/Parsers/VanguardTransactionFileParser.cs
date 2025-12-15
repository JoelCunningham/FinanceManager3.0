using CsvHelper.Configuration;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Parsers.Base;

namespace FinanceManager.Infrastructure.Parsers
{
    public class VanguardTransactionFileParser : BaseTransactionFileParser
    {
        public override string GetDisplayName() => "Vanguard";
        public override IEnumerable<string> GetSupportedFileExtensions() => [".csv"];
        protected override ClassMap<BankRecord> GetClassMap() => new VanguardBankRecordMap();

        private sealed class VanguardBankRecordMap : ClassMap<BankRecord>
        {
            public VanguardBankRecordMap()
            {
                Map(m => m.Date).Name("Date");
                Map(m => m.Narrative).Name("Product Name");
                Map(m => m.CreditAmount).Name("Total");
                Map(m => m.Category).Name("Type");
            }
        }
    }
}