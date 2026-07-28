namespace FinanceManager.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public ApplicationUser(string name, string email) : base(email)
    {
        Name = name;
        Email = email;
    }
}