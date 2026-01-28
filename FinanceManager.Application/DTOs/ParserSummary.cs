namespace FinanceManager.Application.DTOs
{
    public class ParserViewData
    {
        public required string BankName { get; set; }
        public required IReadOnlyList<string> SupportedExtensions { get; set; }
    }
}
