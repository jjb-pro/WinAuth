using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using WinAuth.Messages;
using WinAuth.Models;
using WinAuth.Services;

namespace WinAuth.ViewModels;

public partial class AccountsViewModel : ObservableRecipient, IRecipient<NewTotpMessage>
{
    private readonly ISecretsService _secretsService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<TotpEntry> Entries { get; }

    [ObservableProperty] private TotpEntry? _selectedEntry;

    public AccountsViewModel(ISecretsService secretsService, INavigationService navigationService, IDialogService dialogService)
    {
        _secretsService = secretsService;
        _navigationService = navigationService;
        _dialogService = dialogService;

        Entries = new(_secretsService.LoadAllEntries());

        IsActive = true;
    }

    protected override void OnActivated() => WeakReferenceMessenger.Default.RegisterAll(this);

    protected override void OnDeactivated() => WeakReferenceMessenger.Default.UnregisterAll(this);

    public async void Receive(NewTotpMessage message)
    {
        var newEntry = message.Entry;

        if (Entries.Any(e => newEntry.Id == e.Id))
        {
            await _dialogService.ShowErrorDialogAsync(
                "Duplicate Account",
                $"The account \"{newEntry.Account}\" for issuer \"{newEntry.Issuer}\" has already been added.");
        }
        else
        {
            Entries.Add(newEntry);
        }
    }

    [RelayCommand]
    private void OnAddEntry() => _navigationService.NavigateTo<AddAccountViewModel>();

    [RelayCommand]
    private void OnRemoveEntry(TotpEntry entry)
    {
        _secretsService.DeleteEntry(entry.Id);
        Entries.Remove(entry);
    }

    [RelayCommand]
    private void OnGetOTP(TotpEntry entry)
    {
        SelectedEntry = entry;
    }

    public void OnEntrySelected()
    {
        if (null == SelectedEntry)
            return;
        
        _navigationService.NavigateTo<OTPCodeViewModel>(TransitionAnimation.Slide);
        WeakReferenceMessenger.Default.Send(new UseTotpMessage(SelectedEntry));

        SelectedEntry = null;
    }
}