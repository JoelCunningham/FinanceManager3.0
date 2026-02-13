namespace FinanceManager.Infrastructure.Repositories.InMemory;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public class CategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories;
    private readonly List<CategoryGroup> _categoryGroups;

    public CategoryRepository()
    {
        _categoryGroups = [.. GetDefaultCategoryGroups()];
        _categories = [.. GetDefaultCategories()];
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return [.. _categories];
    }

    private IEnumerable<Category> GetDefaultCategories()
    {
        return
        [
            new() { Id = Guid.NewGuid(), Name = "Salary", Group = GetGroup("Employment") },
            new() { Id = Guid.NewGuid(), Name = "Freelance", Group = GetGroup("Employment") },
            new() { Id = Guid.NewGuid(), Name = "Bonus", Group = GetGroup("Employment") },

            new() { Id = Guid.NewGuid(), Name = "Dividends", Group = GetGroup("Investment") },
            new() { Id = Guid.NewGuid(), Name = "Interest", Group = GetGroup("Investment") },
            new() { Id = Guid.NewGuid(), Name = "Rental Income", Group = GetGroup("Investment") },

            new() { Id = Guid.NewGuid(), Name = "Allowance", Group = GetGroup("Support") },
            new() { Id = Guid.NewGuid(), Name = "Umemployment", Group = GetGroup("Support") },
            new() { Id = Guid.NewGuid(), Name = "Pension", Group = GetGroup("Support") },

            new() { Id = Guid.NewGuid(), Name = "Doctor", Group = GetGroup("Health") },
            new() { Id = Guid.NewGuid(), Name = "Dentist", Group = GetGroup("Health") },
            new() { Id = Guid.NewGuid(), Name = "Optometrist", Group = GetGroup("Health") },
            new() { Id = Guid.NewGuid(), Name = "Therapy", Group = GetGroup("Health") },
            new() { Id = Guid.NewGuid(), Name = "Medication", Group = GetGroup("Health") },
            new() { Id = Guid.NewGuid(), Name = "Supplements", Group = GetGroup("Health") },
            new() { Id = Guid.NewGuid(), Name = "Exercise Equipment", Group = GetGroup("Health") },
            new() { Id = Guid.NewGuid(), Name = "Sport", Group = GetGroup("Health") },
            new() { Id = Guid.NewGuid(), Name = "Insurance", Group = GetGroup("Health") },
            new() { Id = Guid.NewGuid(), Name = "Ambulance Cover", Group = GetGroup("Health") },

            new() { Id = Guid.NewGuid(), Name = "Tuition", Group = GetGroup("Education") },
            new() { Id = Guid.NewGuid(), Name = "Supplies", Group = GetGroup("Education") },
            new() { Id = Guid.NewGuid(), Name = "Fees", Group = GetGroup("Education") },

            new() { Id = Guid.NewGuid(), Name = "Games", Group = GetGroup("Recreation") },
            new() { Id = Guid.NewGuid(), Name = "Activites", Group = GetGroup("Recreation") },
            new() { Id = Guid.NewGuid(), Name = "Alcohol", Group = GetGroup("Recreation") },
            new() { Id = Guid.NewGuid(), Name = "Books", Group = GetGroup("Recreation") },
            new() { Id = Guid.NewGuid(), Name = "Streaming Services", Group = GetGroup("Recreation") },
            new() { Id = Guid.NewGuid(), Name = "Music", Group = GetGroup("Recreation") },

            new() { Id = Guid.NewGuid(), Name = "Electricity", Group = GetGroup("Utilities") },
            new() { Id = Guid.NewGuid(), Name = "Water", Group = GetGroup("Utilities") },
            new() { Id = Guid.NewGuid(), Name = "Gas", Group = GetGroup("Utilities") },
            new() { Id = Guid.NewGuid(), Name = "Internet", Group = GetGroup("Utilities") },
            new() { Id = Guid.NewGuid(), Name = "Phone", Group = GetGroup("Utilities") },

            new() { Id = Guid.NewGuid(), Name = "Public Transport", Group = GetGroup("Transportation") },
            new() { Id = Guid.NewGuid(), Name = "Fuel", Group = GetGroup("Transportation") },
            new() { Id = Guid.NewGuid(), Name = "Maintenance", Group = GetGroup("Transportation") },
            new() { Id = Guid.NewGuid(), Name = "Parking", Group = GetGroup("Transportation") },
            new() { Id = Guid.NewGuid(), Name = "Tolls", Group = GetGroup("Transportation") },
            new() { Id = Guid.NewGuid(), Name = "Taxi", Group = GetGroup("Transportation") },
            new() { Id = Guid.NewGuid(), Name = "Car Hire", Group = GetGroup("Transportation") },
            new() { Id = Guid.NewGuid(), Name = "Roadside Assistance", Group = GetGroup("Transportation") },
            new() { Id = Guid.NewGuid(), Name = "Registration", Group = GetGroup("Transportation") },
            new() { Id = Guid.NewGuid(), Name = "Insurance", Group = GetGroup("Transportation") },
            new() { Id = Guid.NewGuid(), Name = "Flights", Group = GetGroup("Transportation") },

            new() { Id = Guid.NewGuid(), Name = "Groceries", Group = GetGroup("Food") },
            new() { Id = Guid.NewGuid(), Name = "Dining Out", Group = GetGroup("Food") },
            new() { Id = Guid.NewGuid(), Name = "Coffee", Group = GetGroup("Food") },

            new() { Id = Guid.NewGuid(), Name = "EFTs", Group = GetGroup("Investment") },
            new() { Id = Guid.NewGuid(), Name = "Stocks", Group = GetGroup("Investment") },
            new() { Id = Guid.NewGuid(), Name = "Real Estate", Group = GetGroup("Investment") },
            new() { Id = Guid.NewGuid(), Name = "Superannuation", Group = GetGroup("Investment") },

            new() { Id = Guid.NewGuid(), Name = "Mortgage", Group = GetGroup("Housing") },
            new() { Id = Guid.NewGuid(), Name = "Rent", Group = GetGroup("Housing") },
            new() { Id = Guid.NewGuid(), Name = "Rates & Taxes", Group = GetGroup("Housing") },
            new() { Id = Guid.NewGuid(), Name = "Maintenance", Group = GetGroup("Housing") },
            new() { Id = Guid.NewGuid(), Name = "Furniture", Group = GetGroup("Housing") },
            new() { Id = Guid.NewGuid(), Name = "Accommodation", Group = GetGroup("Housing") },

            new() { Id = Guid.NewGuid(), Name = "Clothing", Group = GetGroup("Personal") },
            new() { Id = Guid.NewGuid(), Name = "Cosmetics", Group = GetGroup("Personal") },
            new() { Id = Guid.NewGuid(), Name = "Grooming", Group = GetGroup("Personal") },
            new() { Id = Guid.NewGuid(), Name = "Quality", Group = GetGroup("Personal") },

            new() { Id = Guid.NewGuid(), Name = "Bank Fees", Group = GetGroup("Financial") },
            new() { Id = Guid.NewGuid(), Name = "Credit Card Fees", Group = GetGroup("Financial") },
            new() { Id = Guid.NewGuid(), Name = "Accountant", Group = GetGroup("Financial") },

            new() { Id = Guid.NewGuid(), Name = "Presents", Group = GetGroup("Gifts") },
            new() { Id = Guid.NewGuid(), Name = "Charity", Group = GetGroup("Gifts") },
            new() { Id = Guid.NewGuid(), Name = "Donations", Group = GetGroup("Gifts") },
        ];
    }

    private CategoryGroup GetGroup(string name)
    {
        return _categoryGroups.Find(g => g.Name == name) ?? throw new InvalidOperationException($"Group with name '{name}' not found.");
    }

    private static IEnumerable<CategoryGroup> GetDefaultCategoryGroups()
    {
        return
        [
            new() { Id = Guid.NewGuid(), Name = "Employment", IsIncome = true},
            new() { Id = Guid.NewGuid(), Name = "Investment", IsIncome = true },
            new() { Id = Guid.NewGuid(), Name = "Support", IsIncome = true },
            new() { Id = Guid.NewGuid(), Name = "Health" },
            new() { Id = Guid.NewGuid(), Name = "Education" },
            new() { Id = Guid.NewGuid(), Name = "Recreation" },
            new() { Id = Guid.NewGuid(), Name = "Utilities" },
            new() { Id = Guid.NewGuid(), Name = "Transportation" },
            new() { Id = Guid.NewGuid(), Name = "Food" },
            new() { Id = Guid.NewGuid(), Name = "Investment" },
            new() { Id = Guid.NewGuid(), Name = "Housing" },
            new() { Id = Guid.NewGuid(), Name = "Personal" },
            new() { Id = Guid.NewGuid(), Name = "Financial" },
            new() { Id = Guid.NewGuid(), Name = "Gifts" },
        ];
    }
}