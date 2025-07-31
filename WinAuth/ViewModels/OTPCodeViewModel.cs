#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Timers;
using WinAuth.Helpers;
using WinAuth.Messages;
using WinAuth.Models;
using WinAuth.Services;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace WinAuth.ViewModels;

public partial class OTPCodeViewModel : ObservableRecipient, IRecipient<UseTotpMessage>
{
    private readonly INavigationService _navigationService;

    private readonly Timer _timer = new(1000);

    [ObservableProperty] private TotpEntry? _entry;

    [ObservableProperty] private string _code = "--- ---";
    [ObservableProperty] private int _secondsLeft = 0;
    [ObservableProperty] private int _progress = 0;

    public OTPCodeViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        IsActive = true;
    }

    protected override void OnActivated() => WeakReferenceMessenger.Default.RegisterAll(this);

    protected override void OnDeactivated() => WeakReferenceMessenger.Default.UnregisterAll(this);

    public void Receive(UseTotpMessage message)
    {
        Entry = message.Entry;
        StartTimer();
    }

    private void StartTimer()
    {
        _timer.Elapsed += (s, e) => UpdateTotp();
        _timer.Start();

        UpdateTotp(); // initial update
    }

    private void StopTimer()
    {
        _timer.Elapsed -= (s, e) => UpdateTotp();
        _timer.Stop();
    }

    private async void UpdateTotp()
    {
        if (Entry == null)
            return;

        var period = Entry.Period > 0 ? Entry.Period : 30;
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var secondsPast = (int)(unixTime % period);
        var secondsLeft = period - secondsPast;

        var newCode = TotpHelper.GenerateTOTP(Entry);

        // update properties on UI thread
        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            Code = newCode;
            SecondsLeft = secondsLeft;
            Progress = (int)(secondsPast / (float)period * 100);
        });
    }

    [RelayCommand]
    private void OnGoBack()
    {
        StopTimer();
        _navigationService.GoBack();
    }
}