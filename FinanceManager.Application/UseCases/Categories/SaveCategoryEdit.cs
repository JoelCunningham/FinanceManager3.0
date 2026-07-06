namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using Microsoft.Extensions.Logging;

public sealed record SaveCategoryEditResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SaveCategoryEdit(ICategoryRepository categoryRepository, IDataStore dataStore, ILogger<SaveCategoryEdit> logger)
{
    public async Task<SaveCategoryEditResult> ExecuteAsync(CategorySummary category)
    {
        if (string.IsNullOrEmpty(category.Name)) {
            return new SaveCategoryEditResult([new UseCaseValidationError(category.Id, ValidationField.Name, "Name is required.")]);
        }
        if (string.IsNullOrEmpty(category.Colour))
        {
            return new SaveCategoryEditResult([new UseCaseValidationError(category.Id, ValidationField.Colour, "Colour is required.")]);
        }

        try
        {
            var entity = await categoryRepository.GetOrDefaultAsync(category.Id);

            if (entity is null)
            {
                if (await categoryRepository.ExistsWithNameAsync(category.Name, category.GroupId))
                {
                    return new SaveCategoryEditResult([new UseCaseValidationError(category.Id, ValidationField.Name, $"A category with the name '{category.Name}' already exists in this group.")]);
                }

                entity = new()
                {
                    Id = category.Id,
                    Name = category.Name,
                    Colour = category.Colour,
                    GroupId = category.GroupId,
                    Group = null!
                };

                await categoryRepository.CreateAsync(entity);
            }
            else
            {
                entity.Name = category.Name;
                entity.Colour = category.Colour;
                entity.GroupId = category.GroupId;

                await categoryRepository.UpdateAsync(entity);
            }

            await dataStore.SaveAsync();
            return new SaveCategoryEditResult([]);
        }
        catch
        {
            logger.LogError("An unexpected error occurred while saving category with ID {CategoryId}.", category.Id);
            return new SaveCategoryEditResult([new UseCaseUnexpectedError()]);
        }
    }
}

