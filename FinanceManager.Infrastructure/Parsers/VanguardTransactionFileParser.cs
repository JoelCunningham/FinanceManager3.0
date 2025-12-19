using CsvHelper.Configuration;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Parsers.Base;

namespace FinanceManager.Infrastructure.Parsers
{
    public class VanguardTransactionFileParser : CsvTransactionFileParser
    {
        public override string GetBankName() => "Vanguard";
        public override IEnumerable<string> GetFileExtensions() => [".csv"];
       
        protected override ClassMap<BankRecord> GetClassMap() => new VanguardBankRecordMap();
        private sealed class VanguardBankRecordMap : ClassMap<BankRecord>
        {
            public VanguardBankRecordMap()
            {
                Map(m => m.Date).Name("Date");
                Map(m => m.Narrative).Name("Product Name");
                Map(m => m.CreditAmount!).Name("Total").Optional();
                Map(m => m.Category).Name("Type");
            }
        }
    }
}