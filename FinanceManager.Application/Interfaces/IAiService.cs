using FinanceManager.Application.DTOs;

namespace FinanceManager.Application.Interfaces
{
    public interface IAiService
    {
        Task<string> GenerateCategorySuggestionAsync(ReviewTransaction transaction);
    }
}
