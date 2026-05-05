namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;

public interface IMachineLearningRepository
{
    Task CreateAsync(Guid categoryId, string description);
    Task<IEnumerable<MachineLearning>> GetAllAsync();
    Task<MachineLearning?> GetExactOrDefaultAsync(string description);
    string? NormaliseDescription(string description);
}