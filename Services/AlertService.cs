using Microsoft.JSInterop;

namespace IPrint.Services
{
    public class AlertService(IJSRuntime jsRuntime)
    {
        public async void ShowAlert(string title, string message)
        {
            await jsRuntime.InvokeVoidAsync("showAlert", title, message);
        }
    }
}
