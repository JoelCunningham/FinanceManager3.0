using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;

namespace FinanceManager.WebApp.Models.Base;

public class ValidationModel
{
    public IHxMessengerService? Messenger { get; set; }
    public ValidationType Type { get; set; }
    public string? Message { get; set; }
    public List<ValidationItem> Items { get; set; } = [];

    public void ClearValidation()
    {
        Items = [];
        Message = null;
        Type = ValidationType.None;
        Messenger?.Clear();
    }

    public void SetState(ValidationType type, string? message = null, bool isSilent = false)
    {
        Type = type;
        Message = message;
        if (!isSilent) ShowMessage();
    }

    public void SetSuccess(string message, bool isSilent = false)
    {
        SetState(ValidationType.Success, message, isSilent);
    }

    public void SetInfo(string message, bool isSilent = false)
    {
        SetState(ValidationType.Info, message, isSilent);
    }

    public void SetError(string message, bool isSilent = false)
    {
        SetState(ValidationType.Error, message, isSilent);
    }

    public void SetError(Guid itemId, string field, string message)
    {
        Items.Add(new() { Id = itemId, Field = field });
        SetState(ValidationType.Error, message);
    }

    public List<Guid> GetValidationItemIds(string field)
    {
        return [.. Items.Where(i => i.Field == field).Select(i => i.Id)];
    }

    public string GetControlClass()
    {
        return Type switch
        {
            ValidationType.Error => "is-invalid",
            ValidationType.Success => "is-valid",
            _ => string.Empty
        };
    }

    public string GetFeedbackClass()
    {
        return Type switch
        {
            ValidationType.Error => "invalid-feedback",
            ValidationType.Success => "valid-feedback",
            _ => string.Empty
        };
    }

    private void ShowMessage()
    {
        if (Messenger is null || Message is null) return;

        switch (Type)
        {
            case ValidationType.Error:
                Messenger.AddError(Message);
                break;

            case ValidationType.Success:
                Messenger.AddInformation(Message);
                break;

            default:
                Messenger.AddInformation(Message);
                break;
        }
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