namespace FinanceManager.Application.Common;

public class UserState
{
    public string? Name { get; set; }

    public event Action? OnNameChange;

    public void UpdateUserName(string? name)
    {
        Name = name;
        OnNameChange?.Invoke();
    }
}