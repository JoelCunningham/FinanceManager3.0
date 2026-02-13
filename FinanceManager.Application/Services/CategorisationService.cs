namespace FinanceManager.Application.Services;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public class CategorisationService(IMachineLearningRepository MachineLearningRepository, ICategoryRepository CategoryRepository)
{
    public async Task<Category?> SuggestCategory(ReviewTransaction transaction)
    {
        var normalisedDescription = MachineLearningRepository.NormaliseDescription(transaction.Description);

        if (string.IsNullOrWhiteSpace(normalisedDescription)) return null;

        var exactMatch = await MachineLearningRepository.GetExactOrDefaultAsync(normalisedDescription);
        if (exactMatch is not null) return exactMatch.Category;

        var allCategories = await CategoryRepository.GetAllAsync();
        var allMemories = await MachineLearningRepository.GetAllAsync();

        var similarCategory = SuggestBySimilarity(normalisedDescription, allCategories, allMemories);
        if (similarCategory != null) return similarCategory;

        return null;
    }

    public async Task<bool> HasMemories()
    {
        var memories = await MachineLearningRepository.GetAllAsync();
        return memories.Any();
    }

    private Category? SuggestBySimilarity(string description, IEnumerable<Category> categories, IEnumerable<MachineLearning> memories)
    {
        var categoryScores = new Dictionary<Category, int>();

        var descriptionTokens = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var firstDescriptionToken = descriptionTokens.FirstOrDefault();

        foreach (var category in categories)
        {
            categoryScores.Add(category, 0);

            var normalisedCategory = MachineLearningRepository.NormaliseDescription(category.Name);
            if (string.IsNullOrWhiteSpace(normalisedCategory)) continue;

            if (description.Contains(normalisedCategory.PadLeft(' ').PadRight(' ')))
            {
                categoryScores[category] += 3;
            }
            else
            {
                var categoryTokens = normalisedCategory.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                categoryScores[category] += categoryTokens.Intersect(descriptionTokens).Count() * 2;
            }
        }

        foreach (var memory in memories)
        {
            var known = memory.NormalisedDescription;
            var knownTokens = known.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Shared words
            categoryScores[memory.Category] += knownTokens.Intersect(descriptionTokens).Count();

            // First word match
            if (knownTokens.FirstOrDefault() == firstDescriptionToken)
            {
                categoryScores[memory.Category] += 3;
            }

            // Substring match
            if (known.Contains(description) || description.Contains(known))
            {
                categoryScores[memory.Category] += 2;
            }
        }

        if (categoryScores.Values.All(score => score < 2)) return null;

        return categoryScores.OrderByDescending(kvp => kvp.Value).First().Key;
    }
}