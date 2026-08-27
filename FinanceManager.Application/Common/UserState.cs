namespace FinanceManager.Application.Common;

using FinanceManager.Application.Enums;

public class UserState
{
    public string? Name { get; set; }
    public ColourTheme? Theme { get; set; }

    public event Action? OnNameChange;
    public event Action? OnThemeChange;

    public void UpdateUserName(string? name)
    {
        Name = name;
        OnNameChange?.Invoke();
    }

    public void UpdateTheme(ColourTheme? theme)
    {
        Theme = theme;
        OnThemeChange?.Invoke();
    }
}