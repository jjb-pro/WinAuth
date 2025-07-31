#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using WinAuth.Messages;
using WinAuth.Models;
using WinAuth.Services;

namespace WinAuth.ViewModels;

public partial class AccountsViewModel(ISecretsService secretsService, INavigationService navigationService) : ObservableObject
{
    public ObservableCollection<TotpEntry> Entries { get; } = new(secretsService.LoadAllEntries());

    [ObservableProperty]
    private TotpEntry? _selectedEntry;

    [RelayCommand]
    private void OnAddEntry() => navigationService.NavigateTo<AddAccountViewModel>();

    [RelayCommand]
    private void OnRemoveEntry(TotpEntry entry)
    {
        secretsService.RemoveEntry(entry.Id);
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

        navigationService.NavigateTo<OTPCodeViewModel>();
        WeakReferenceMessenger.Default.Send(new UseTotpMessage(SelectedEntry));

        SelectedEntry = null;
    }
}