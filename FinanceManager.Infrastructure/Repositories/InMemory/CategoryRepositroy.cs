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

    public Task<Category> GetByIdAsync(Guid id)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category is null) throw new KeyNotFoundException("Category not found");

        return Task.FromResult(category);
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
            CreateCategory("Unemployment", "Support"),
            CreateCategory("Pension", "Support"),

            CreateCategory("Medical Services", "Health"),
            CreateCategory("Medication", "Health"),
            CreateCategory("Supplements", "Health"),
            CreateCategory("Fitness", "Health"),
            CreateCategory("Insurance", "Health"),
            CreateCategory("Ambulance Cover", "Health"),

            CreateCategory("Entertainment", "Recreation"),
            CreateCategory("Activities", "Recreation"),
            CreateCategory("Alcohol", "Recreation"),
            CreateCategory("Books", "Recreation"),

            CreateCategory("Electricity", "Utilities"),
            CreateCategory("Water", "Utilities"),
            CreateCategory("Gas", "Utilities"),
            CreateCategory("Internet", "Utilities"),
            CreateCategory("Phone", "Utilities"),

            CreateCategory("Public Transport", "Transportation"),
            CreateCategory("Fuel", "Transportation"),
            CreateCategory("Maintenance", "Transportation"),
            CreateCategory("Taxi", "Transportation"),
            CreateCategory("Fees", "Transportation"),
            CreateCategory("Registration", "Transportation"),
            CreateCategory("Insurance", "Transportation"),
            CreateCategory("Roadside Assistance", "Transportation"),
            CreateCategory("Flights", "Transportation"),
            CreateCategory("Car Hire", "Transportation"),

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
            CreateCategory("Lifestyle", "Personal"),
            CreateCategory("Tuition", "Personal"),

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
        if (group is null)
        {
            throw new NotImplementedException();
        }

        var category = new Category
        {

            Id = Guid.NewGuid(),
            Name = name,
            Group = group,
            GroupId = group.Id,
            Colour = GetRandomColour()
        };

        group.Categories ??= [];
        group.Categories.Add(category);

        return category;
    }

    private static IEnumerable<CategoryGroup> GetDefaultCategoryGroups()
    {
        return
        [
            new() { Id = Guid.NewGuid(), Name = "Employment", IsIncome = true, Colour = colours[0] },
            new() { Id = Guid.NewGuid(), Name = "Investment", IsIncome = true, Colour = colours[1] },
            new() { Id = Guid.NewGuid(), Name = "Support", IsIncome = true, Colour = colours[2] },

            new() { Id = Guid.NewGuid(), Name = "Financial", Colour = colours[0] },
            new() { Id = Guid.NewGuid(), Name = "Food", Colour = colours[1] },
            new() { Id = Guid.NewGuid(), Name = "Gifts", Colour = colours[2] },
            new() { Id = Guid.NewGuid(), Name = "Health", Colour = colours[3] },
            new() { Id = Guid.NewGuid(), Name = "Housing", Colour = colours[4] },
            new() { Id = Guid.NewGuid(), Name = "Investing", Colour = colours[5] },
            new() { Id = Guid.NewGuid(), Name = "Personal", Colour = colours[6] },
            new() { Id = Guid.NewGuid(), Name = "Recreation", Colour = colours[7] },
            new() { Id = Guid.NewGuid(), Name = "Transportation", Colour = colours[8] },
            new() { Id = Guid.NewGuid(), Name = "Utilities", Colour = colours[9] },
        ];
    }

    private static string GetRandomColour()
    {
        return colours[new Random().Next(colours.Length)];
    }

    private static readonly string[] colours = 
    [
        "#54478C", "#2C699A", "#048BA8", "#0DB39E", "#16DB93", "#83E377", "#B9E769", "#EFEA5A", "#F1C453", "#F29E4C"
    ];
}