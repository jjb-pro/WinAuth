using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace WinAuth.Services.Implementations;

public class DialogService : IDialogService
{
    public Task ShowErrorDialogAsync(string title, string content)
    {
        var messageDialog = new MessageDialog(content, title);
        return messageDialog.ShowAsync().AsTask();
    }
}