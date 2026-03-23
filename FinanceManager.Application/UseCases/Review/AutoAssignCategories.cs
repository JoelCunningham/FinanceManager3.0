namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public sealed record AutoCategoriseResult(int AssignedCount) : UseCaseResult;

public sealed class AutoAssignCategories(ICategoryRepository categoryRepository, IMachineLearningRepository machineLearningRepository)
{
    public async Task<AutoCategoriseResult> ExecuteAsync(IEnumerable<ReviewGroup> groups, IEnumerable<CategorySummary> categories)
    {
        int assignedCount = 0;
        var categoryDict = categories.ToDictionary(c => c.Id);

        foreach (var group in groups)
        {
            foreach (var transaction in group.Transactions)
            {
                var suggested = await SuggestCategoryAsync(transaction.Description);
                if (suggested is not null && categoryDict.TryGetValue(suggested.Value, out var category))
                {
                    transaction.Category = category;
                    transaction.IsAutoCategorised = true;
                    assignedCount++;
                }
            }
        }

        return new AutoCategoriseResult(assignedCount);
    }

    private async Task<Guid?> SuggestCategoryAsync(string description)
    {
        var normalisedDescription = machineLearningRepository.NormaliseDescription(description);

        if (string.IsNullOrWhiteSpace(normalisedDescription)) return null;

        var exactMatch = await machineLearningRepository.GetExactOrDefaultAsync(normalisedDescription);
        if (exactMatch is not null) return exactMatch.Category.Id;

        var allCategories = await categoryRepository.GetAllAsync();
        var allMemories = await machineLearningRepository.GetAllAsync();

        var similarCategory = SuggestBySimilarity(normalisedDescription, allCategories, allMemories);
        if (similarCategory != null) return similarCategory.Id;

        return null;
    }

    private Category? SuggestBySimilarity(string description, IEnumerable<Category> categories, IEnumerable<MachineLearning> memories)
    {
        var categoryScores = new Dictionary<Guid, int>();

        var descriptionTokens = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var firstDescriptionToken = descriptionTokens.FirstOrDefault();

        foreach (var category in categories)
        {
            categoryScores.Add(category.Id, 0);

            var normalisedCategory = machineLearningRepository.NormaliseDescription(category.Name);
            if (string.IsNullOrWhiteSpace(normalisedCategory)) continue;

            if (description.Contains(normalisedCategory.PadLeft(' ').PadRight(' ')))
            {
                categoryScores[category.Id] += 3;
            }
            else
            {
                var categoryTokens = normalisedCategory.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                categoryScores[category.Id] += categoryTokens.Intersect(descriptionTokens).Count() * 2;
            }
        }

        foreach (var memory in memories)
        {
            var known = memory.NormalisedDescription;
            var knownTokens = known.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            categoryScores[memory.Category.Id] += knownTokens.Intersect(descriptionTokens).Count();

            if (knownTokens.FirstOrDefault() == firstDescriptionToken)
            {
                categoryScores[memory.Category.Id] += 3;
            }

            if (known.Contains(description) || description.Contains(known))
            {
                categoryScores[memory.Category.Id] += 2;
            }
        }

        if (categoryScores.Values.All(score => score < 2)) return null;
        var mostLikelyCategoryId = categoryScores.OrderByDescending(kvp => kvp.Value).First().Key;

        return categories.FirstOrDefault(c => c.Id == mostLikelyCategoryId);
    }
}
