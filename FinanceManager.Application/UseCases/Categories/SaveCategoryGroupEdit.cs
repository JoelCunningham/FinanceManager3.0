namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record SaveCategoryGroupEditResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SaveCategoryGroupEdit(ICategoryGroupRepository categoryGroupRepository, IDataStore dataStore)
{
    public async Task<SaveCategoryGroupEditResult> ExecuteAsync(CategoryGroupSummary group)
    {
        try
        {
            var entity = await categoryGroupRepository.GetOrDefaultAsync(group.Id);

            if (entity is null)
            {
                entity = new()
                {
                    Id = group.Id,
                    Name = group.Name,
                    Colour = group.Colour,
                    IsIncome = group.IsIncome,
                };

                await categoryGroupRepository.CreateAsync(entity);
            }
            else
            {
                entity.Name = group.Name;
                entity.Colour = group.Colour;
                entity.IsIncome = group.IsIncome;

                await categoryGroupRepository.UpdateAsync(entity);
            }

            await dataStore.SaveAsync();
            return new SaveCategoryGroupEditResult([]);
        }
        catch
        {
            // TODO: Log exception
            return new SaveCategoryGroupEditResult([new UseCaseUnexpectedError()]);
        }
    }
}

