using FinanceManager.Application.Interfaces;

namespace FinanceManager.Application.DTOs
{
    public class ParserSummary
    {
        public required string BankName { get; set; }
        public required IReadOnlyList<string> SupportedExtensions { get; set; }

        public static IEnumerable<ParserSummary> FromParsers(IEnumerable<ITransactionFileParser> parsers)
        {
            return [..parsers
                .GroupBy(parser => parser.GetBankName(), StringComparer.OrdinalIgnoreCase)
                .Select(group => new ParserSummary
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
    }
}
