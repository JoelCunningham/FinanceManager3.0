namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record SaveCategoryEditResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SaveCategoryEdit(ICategoryRepository categoryRepository, IDataStore dataStore)
{
    public async Task<SaveCategoryEditResult> ExecuteAsync(CategorySummary category)
    {
        try
        {
            var entity = await categoryRepository.GetOrDefaultAsync(category.Id);

            if (entity is null)
            {
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
            // TODO: Log exception
            return new SaveCategoryEditResult([new UseCaseUnexpectedError()]);
        }
    }
}

