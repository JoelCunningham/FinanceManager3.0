namespace FinanceManager.WebApp.Models.Base;

public class ValidationModel
{
    public List<ErrorItem> Items { get; set; } = [];
    public string? ErrorMessage { get; set; }
    public bool HasSuccess { get; set; } = false;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage) || Items.Count > 0;

    public void ClearErrors()
    {
        Items = [];
        ErrorMessage = null;
    }

    public void SetError(string message)
    {
        SetErrorMessage(message);
    }

    public void SetError(Guid itemId, string message)
    {
        Items.Add(new() { Id = itemId });
        SetErrorMessage(message);
    }

    public void SetError(Guid itemId, string field, string message)
    {
        Items.Add(new() { Id = itemId, Field = field });
        SetErrorMessage(message);
    }

    public void SetSucess()
    {
        ClearErrors();
        HasSuccess = true;
    }

    public List<Guid> GetErrorItemIds(string field)
    {
        return [.. Items.Where(i => i.Field == field).Select(i => i.Id)];
    }

    private void SetErrorMessage(string message)
    {
        ErrorMessage = ErrorMessage is null ? message : "Multiple problems detected";
    }
}

public class ErrorItem
{
    public Guid Id { get; set; }
    public string Field { get; set; } = string.Empty;
}