namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public sealed record SuggestCategoryResult(Guid? CategoryId = null) : UseCaseResult;

public sealed class SuggestCategory(ICategoryRepository categoryRepository, IMachineLearningRepository machineLearningRepository)
{
    public async Task<SuggestCategoryResult> ExecuteAsync(string description)
    {
        var normalisedDescription = machineLearningRepository.NormaliseDescription(description);

        if (string.IsNullOrWhiteSpace(normalisedDescription)) return new SuggestCategoryResult();

        var exactMatch = await machineLearningRepository.GetExactOrDefaultAsync(normalisedDescription);
        if (exactMatch is not null) return new SuggestCategoryResult(exactMatch.Category.Id);

        var allCategories = await categoryRepository.GetAllAsync();
        var allMemories = await machineLearningRepository.GetAllAsync();

        var similarCategory = SuggestBySimilarity(normalisedDescription, allCategories, allMemories);
        if (similarCategory != null) return new SuggestCategoryResult(similarCategory.Id);

        return new SuggestCategoryResult();
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

            // Shared words
            categoryScores[memory.Category.Id] += knownTokens.Intersect(descriptionTokens).Count();

            // First word match
            if (knownTokens.FirstOrDefault() == firstDescriptionToken)
            {
                categoryScores[memory.Category.Id] += 3;
            }

            // Substring match
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
