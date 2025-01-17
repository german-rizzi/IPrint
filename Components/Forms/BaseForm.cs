using Microsoft.AspNetCore.Components;

public class BaseForm<TItem> : ComponentBase
{
    public TItem? Data { get; set; }

    public Func<TItem, bool>? SubmitValidation { get; set; }

    public EventCallback<TItem?> OnSubmit { get; set; }
}