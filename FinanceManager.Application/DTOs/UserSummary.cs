namespace FinanceManager.Application.DTOs;

public class UserSummary(string email, string name)
{
    public string Email { get; init; } = email;
    public string Name { get; init; } = name;
}
