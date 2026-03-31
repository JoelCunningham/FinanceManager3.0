namespace FinanceManager.WebApp.Models;

public sealed class EditBudgetEntryModel
{
    public Guid? Id { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public int StartCellIndex { get; set; }
    public int Span { get; set; } = 1;
}