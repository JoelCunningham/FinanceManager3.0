namespace FinanceManager.WebApp.Models.Base;

public class ValidationModel
{
    public ValidationType Type { get; set; }
    public string? ValidationMessage { get; set; }
    public List<ValidationItem> Items { get; set; } = [];

    public bool HasState => Type != ValidationType.None;
    public bool HasError => Type == ValidationType.Error;
    public bool HasSuccess => Type == ValidationType.Success;

    public void ClearValidation()
    {
        Items = [];
        ValidationMessage = null;
        Type = ValidationType.None;
    }

    public void SetError(Guid itemId, string message)
    {
        Items.Add(new() { Id = itemId });
        SetMessage(message);
        Type = ValidationType.Error;
    }

    public void SetError(Guid itemId, string field, string message)
    {
        Items.Add(new() { Id = itemId, Field = field });
        SetMessage(message);
        Type = ValidationType.Error;
    }

    public void SetValidationState(ValidationType type, string? message = null)
    {
        if (!string.IsNullOrEmpty(message))
        {
            SetMessage(message);
        }
        Type = type;
    }

    public List<Guid> GetValidationItemIds(string field)
    {
        return [.. Items.Where(i => i.Field == field).Select(i => i.Id)];
    }

    private void SetMessage(string message)
    {
        if (Type == ValidationType.Error)
        {
            ValidationMessage = ValidationMessage is null ? message : "Multiple problems detected";
        }
        ValidationMessage = message;
    }
}

public class ValidationItem
{
    public Guid Id { get; set; }
    public string Field { get; set; } = string.Empty;
}

public enum ValidationType
{
    None,
    Info,
    Error,
    Success,
}