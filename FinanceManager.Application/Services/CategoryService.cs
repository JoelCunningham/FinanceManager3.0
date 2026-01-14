namespace FinanceManager.Application.Services
{
    public class CategoryService()
    {
        public IEnumerable<string> GetCategories()
        {
            return ["Fuel", "Clothing", "Groceries"];
        }
    }
}
