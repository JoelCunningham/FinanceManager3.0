namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using Havit.Blazor.Components.Web;

public class ValidationModel
{
    public IHxMessengerService? Messenger { get; set; }
    public ValidationType Type { get; set; }
    public string? Message { get; set; }
    public List<ValidationItem> Items { get; set; } = [];

    private MessengerMessage SuccessMessage => CreateMessage("bg-success text-white");
    private MessengerMessage ErrorMessage => CreateMessage("bg-danger text-white");
    private MessengerMessage InformationMessage => CreateMessage("bg-info text-white");

    private MessengerMessage CreateMessage(string cssClass) => new()
    {
        Text = Message ?? string.Empty,
        CssClass = cssClass,
        AutohideDelay = 5000,
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

        MessengerMessage message = Type switch
        {
            ValidationType.Error => ErrorMessage,
            ValidationType.Success => SuccessMessage,
            _ => InformationMessage 
        };
        Messenger.AddMessage(message);
    }
}

public class ValidationItem
{
    public Guid Id { get; set; }
    public ValidationField Field { get; set; }
}