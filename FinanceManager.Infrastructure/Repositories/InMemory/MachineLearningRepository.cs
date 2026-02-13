namespace FinanceManager.Infrastructure.Repositories.InMemory;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public class MachineLearningRepository : IMachineLearningRepository
{
    private readonly List<MachineLearning> _memories = [];

    private static readonly string[] GenericTerms = ["debit", "credit", "card", "payment", "purchase", "withdraw", "withdrawal", "osko", "aus", "paid", "eftpos", "deposit", "sp", "fee", "bonus"];

    public async Task SaveAsync(Category category, string description)
    {
        var normalisedDescription = NormaliseDescription(description);
        if (normalisedDescription == null) return;

        _memories.Add(new MachineLearning 
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Category = category,
            NormalisedDescription = normalisedDescription,
            LastUsed = DateTime.UtcNow
        });
    }

    public async Task<IEnumerable<MachineLearning>> GetAllAsync()
    {
        return _memories; 
    }

    public async Task<MachineLearning?> GetExactOrDefaultAsync(string normalisedDescription)
    {
        return _memories.FirstOrDefault(m => m.NormalisedDescription == normalisedDescription);
    }

    public string? NormaliseDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return null;

        description = description.ToLowerInvariant();
        description = new([.. description.Select(c => char.IsLetter(c) || char.IsWhiteSpace(c) ? c : ' ')]);
        description = string
            .Join(" ", description.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => !GenericTerms.Contains(t))
            .Select(t => t.EndsWith('s') ? t[..^1] : t));


        if (string.IsNullOrWhiteSpace(description)) return null;

        return string.Join(" ", description.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}