namespace FinanceManager.Application.Common;

public class UserState
{
    public string? Name { get; set; }

    public event Action? OnChange;

    public void UpdateUserName(string? name)
    {
        Name = name;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}