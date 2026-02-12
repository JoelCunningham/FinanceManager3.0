namespace FinanceManager.WebApp.Models.Base;

public class ValidationModel
{
    public Guid? ErrorId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage) || ErrorId is not null;

    public void ClearError()
    {
        ErrorId = null;
        ErrorMessage = null;
    }

    public void SetError(string message)
    {
        ErrorMessage = message;
    }

    public void SetError(Guid errorId, string message)
    {
        ErrorId = errorId;
        ErrorMessage = message;
    }
}

