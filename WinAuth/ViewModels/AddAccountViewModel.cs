using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WinAuth.Messages;
using WinAuth.Parsers;
using WinAuth.Services;

namespace WinAuth.ViewModels;

public partial class AddAccountViewModel(ISecretsService secretsService, INavigationService navigationService, IDialogService dialogService) : ObservableObject
{
    [ObservableProperty]
    private string _secret;

    [RelayCommand]
    private void OnGoBack() => navigationService.GoBack();

    public async void OnDetectedCodeChanged()
    {
        try
        {
            var newEntry = TotpUriParser.Parse(Secret);
            secretsService.SaveEntry(newEntry);

            navigationService.GoBack();

            WeakReferenceMessenger.Default.Send(new NewTotpMessage(newEntry));
        }
        catch
        {
            await dialogService.ShowErrorDialogAsync(
                "Unable to add account",
                "We couldn't read the QR code or TOTP configuration. Please ensure it's a valid code and try again."
            );
        }
    }
}