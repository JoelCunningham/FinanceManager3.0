namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;

public interface IMachineLearningRepository
{
    Task SaveAsync(Category category, string description);
    Task<IEnumerable<MachineLearning>> GetAllAsync();
    Task<MachineLearning?> GetExactOrDefaultAsync(string description);
    string? NormaliseDescription(string description);
}