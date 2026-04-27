namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.Constants;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;

public class ValidationModel
{
    public IHxMessengerService? Messenger { get; set; }
    public ValidationType Type { get; set; }
    public string? Message { get; set; }
    public List<ValidationItem> Items { get; set; } = [];

    private MessengerMessage SuccessMessage => new()
    {
        Text = Message ?? string.Empty,
        CssClass = "bg-success text-white",
        AutohideDelay = 5000
    };

    public void Clear()
    {
        Items = [];
        Message = null;
        Type = ValidationType.None;
        Messenger?.Clear();
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

    //TODO Remove this after moving DTO logic to application layer
    public void SetError(Guid itemId, ValidationField field, string message)
    {
        Items.Add(new() { Id = itemId, Field = field });
        SetState(ValidationType.Error, message);
    }

    public void SetError(UseCaseError error)
    {
        if (error is UseCaseValidationError validationError)
        {
            Items.Add(new() { Id = validationError.RecordId, Field = validationError.Field });
        }
        SetState(ValidationType.Error, error.Message);
    }

    public void SetErrors(IEnumerable<UseCaseError> errors)
    {
        foreach (var error in errors)
        {
            SetError(error);
        }
    }

    public List<Guid> GetValidationItemIds(ValidationField field)
    {
        return [.. Items.Where(i => i.Field == field).Select(i => i.Id)];
    }

    public void ClearValidationItem(Guid itemId, ValidationField field)
    {
        Items.RemoveAll(i => i.Id == itemId && i.Field == field);
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

    private void SetState(ValidationType type, string? message = null, bool isSilent = false)
    {
        Type = type;
        Message = message;
        if (!isSilent) ShowMessage();
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
                Messenger.AddMessage(SuccessMessage);
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
    public ValidationField Field { get; set; }
}