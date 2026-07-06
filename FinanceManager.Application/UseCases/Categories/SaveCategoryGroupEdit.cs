namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using Microsoft.Extensions.Logging;

public sealed record SaveCategoryGroupEditResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SaveCategoryGroupEdit(ICategoryGroupRepository categoryGroupRepository, ITransactionRepository transactionRepository, IDataStore dataStore, ILogger<SaveCategoryGroupEdit> logger)
{
    public async Task<SaveCategoryGroupEditResult> ExecuteAsync(CategoryGroupSummary group)
    {
        if (string.IsNullOrEmpty(group.Name))
        {
            return new SaveCategoryGroupEditResult([new UseCaseValidationError(group.Id, ValidationField.Name, "Name is required.")]);
        }
        if (string.IsNullOrEmpty(group.Colour))
        {
            return new SaveCategoryGroupEditResult([new UseCaseValidationError(group.Id, ValidationField.Colour, "Colour is required.")]);
        }
        if (string.IsNullOrEmpty(group.Icon))
        {
            return new SaveCategoryGroupEditResult([new UseCaseValidationError(group.Id, ValidationField.Icon, "Icon is required.")]);
        }

        try
        {
            var entity = await categoryGroupRepository.GetOrDefaultAsync(group.Id);

            if (entity is null)
            {
                if (await categoryGroupRepository.ExistsWithNameAsync(group.Name))
                {
                    return new SaveCategoryGroupEditResult([new UseCaseValidationError(group.Id, ValidationField.Name, $"An area with the name '{group.Name}' already exists.")]);
                }

                entity = new()
                {
                    Id = group.Id,
                    Name = group.Name,
                    Colour = group.Colour,
                    Icon = group.Icon,
                    IsIncome = group.IsIncome,
                };

                await categoryGroupRepository.CreateAsync(entity);
            }
            else
            {
                if (entity.IsIncome != group.IsIncome && await transactionRepository.HasTransactionsForCategoryGroupAsync(entity.Id))
                {
                    return new SaveCategoryGroupEditResult([new UseCaseValidationError(group.Id, ValidationField.IsIncome, $"Is Income cannot be changed as the area contains transactions.")]);
                }

                entity.Name = group.Name;
                entity.Colour = group.Colour;
                entity.Icon = group.Icon;
                entity.IsIncome = group.IsIncome;

                await categoryGroupRepository.UpdateAsync(entity);
            }

            await dataStore.SaveAsync();
            return new SaveCategoryGroupEditResult([]);
        }
        catch
        {
            logger.LogError("An unexpected error occurred while saving category group with ID {GroupId}.", group.Id);
            return new SaveCategoryGroupEditResult([new UseCaseUnexpectedError()]);
        }
    }
}

