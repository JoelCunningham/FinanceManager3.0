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

    public Task<IEnumerable<Category>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Category>>([.. _categories]);
    }

    public Task<IEnumerable<CategoryGroup>> GetAllGroupsAsync()
    {
        return Task.FromResult<IEnumerable<CategoryGroup>>([.. _categoryGroups]);
    }

    private IEnumerable<Category> GetDefaultCategories()
    {
        return
        [
            CreateCategory("Salary", "Employment"),
            CreateCategory("Freelance", "Employment"),
            CreateCategory("Bonus", "Employment"),

            CreateCategory("Dividends", "Investment"),
            CreateCategory("Interest", "Investment"),
            CreateCategory("Rental Income", "Investment"),

            CreateCategory("Allowance", "Support"),
            CreateCategory("Umemployment", "Support"),
            CreateCategory("Pension", "Support"),

            CreateCategory("Doctor", "Health"),
            CreateCategory("Dentist", "Health"),
            CreateCategory("Optometrist", "Health"),
            CreateCategory("Therapy", "Health"),
            CreateCategory("Medication", "Health"),
            CreateCategory("Supplements", "Health"),
            CreateCategory("Exercise Equipment", "Health"),
            CreateCategory("Sport", "Health"),
            CreateCategory("Insurance", "Health"),
            CreateCategory("Ambulance Cover", "Health"),

            CreateCategory("Tuition", "Education"),
            CreateCategory("Supplies", "Education"),
            CreateCategory("Fees", "Education"),

            CreateCategory("Games", "Recreation"),
            CreateCategory("Activites", "Recreation"),
            CreateCategory("Alcohol", "Recreation"),
            CreateCategory("Books", "Recreation"),
            CreateCategory("Streaming Services", "Recreation"),
            CreateCategory("Music", "Recreation"),

            CreateCategory("Electricity", "Utilities"),
            CreateCategory("Water", "Utilities"),
            CreateCategory("Gas", "Utilities"),
            CreateCategory("Internet", "Utilities"),
            CreateCategory("Phone", "Utilities"),

            CreateCategory("Public Transport", "Transportation"),
            CreateCategory("Fuel", "Transportation"),
            CreateCategory("Maintenance", "Transportation"),
            CreateCategory("Parking", "Transportation"),
            CreateCategory("Tolls", "Transportation"),
            CreateCategory("Taxi", "Transportation"),
            CreateCategory("Car Hire", "Transportation"),
            CreateCategory("Roadside Assistance", "Transportation"),
            CreateCategory("Registration", "Transportation"),
            CreateCategory("Insurance", "Transportation"),
            CreateCategory("Flights", "Transportation"),

            CreateCategory("Groceries", "Food"),
            CreateCategory("Dining Out", "Food"),
            CreateCategory("Coffee", "Food"),

            CreateCategory("EFTs", "Investing"),
            CreateCategory("Stocks", "Investing"),
            CreateCategory("Real Estate", "Investing"),
            CreateCategory("Superannuation", "Investing"),

            CreateCategory("Mortgage", "Housing"),
            CreateCategory("Rent", "Housing"),
            CreateCategory("Rates & Taxes", "Housing"),
            CreateCategory("Maintenance", "Housing"),
            CreateCategory("Furniture", "Housing"),
            CreateCategory("Manchester", "Housing"),
            CreateCategory("Accommodation", "Housing"),

            CreateCategory("Hygiene", "Personal"),
            CreateCategory("Clothing", "Personal"),
            CreateCategory("Cosmetics", "Personal"),
            CreateCategory("Grooming", "Personal"),
            CreateCategory("Quality", "Personal"),

            CreateCategory("Bank Fees", "Financial"),
            CreateCategory("Credit Card Fees", "Financial"),
            CreateCategory("Accountant", "Financial"),

            CreateCategory("Presents", "Gifts"),
            CreateCategory("Charity", "Gifts"),
            CreateCategory("Donations", "Gifts"),
        ];
    }

    private Category CreateCategory(string name, string groupName)
    {
        var group = _categoryGroups.Find(g => g.Name == groupName);
        if (group is not null)
        {
            return new() { Id = Guid.NewGuid(), Name = name, Group = group, GroupId = group.Id };
        }
        throw new NotImplementedException();
    }

    private static IEnumerable<CategoryGroup> GetDefaultCategoryGroups()
    {
        return
        [
            new() { Id = Guid.NewGuid(), Name = "Employment", IsIncome = true },
            new() { Id = Guid.NewGuid(), Name = "Investment", IsIncome = true },
            new() { Id = Guid.NewGuid(), Name = "Support", IsIncome = true },
            new() { Id = Guid.NewGuid(), Name = "Health" },
            new() { Id = Guid.NewGuid(), Name = "Education" },
            new() { Id = Guid.NewGuid(), Name = "Recreation" },
            new() { Id = Guid.NewGuid(), Name = "Utilities" },
            new() { Id = Guid.NewGuid(), Name = "Transportation" },
            new() { Id = Guid.NewGuid(), Name = "Food" },
            new() { Id = Guid.NewGuid(), Name = "Investing" },
            new() { Id = Guid.NewGuid(), Name = "Housing" },
            new() { Id = Guid.NewGuid(), Name = "Personal" },
            new() { Id = Guid.NewGuid(), Name = "Financial" },
            new() { Id = Guid.NewGuid(), Name = "Gifts" },
        ];
    }
}