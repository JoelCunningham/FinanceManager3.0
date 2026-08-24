namespace FinanceManager.Application.DTOs;

public class UserSummary(Guid id, string email, string name)
{
    public Guid Id { get; init; } = id;
    public string Email { get; init; } = email;
    public string Name { get; init; } = name;
}