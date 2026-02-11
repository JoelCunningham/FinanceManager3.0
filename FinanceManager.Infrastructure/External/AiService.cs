using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using Google.GenAI;

namespace FinanceManager.Infrastructure.External
{
    public class AiService : IAiService
    {
        public async Task<string> GenerateCategorySuggestionAsync(ReviewTransaction transaction)
        {
            var client = new Client(apiKey: "dsilfuaiopueuiwaopuwea");

            var response = await client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash-lite",
                contents: [new() { Parts = [new()
                {
                    Text = $"Given the following transaction details, suggest a category for it:\n" +
                            $"Description: {transaction.Description}\n" +
                            $"Amount: {transaction.Amount}\n" +
                            $"Date: {transaction.Date:d}"
                }]}]
            );
            var text = response.Candidates?[0].Content?.Parts?[0]?.Text;

            if (string.IsNullOrEmpty(text))
            {
                throw new Exception("AI model returned empty content.");
            }

            return text;
        }
    }
}