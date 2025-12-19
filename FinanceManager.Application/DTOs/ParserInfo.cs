namespace FinanceManager.Application.DTOs
{
    public sealed record ParserInfo(
        string BankName,
        IReadOnlyList<string> SupportedExtensions
    );
}
