using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using WinAuth.Pages;
using WinAuth.Services;
using WinAuth.Services.Implementations;
using WinAuth.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace WinAuth;

sealed partial class App : Application
{
    public IServiceProvider ServiceContainer { get; private set; }

    public App()
    {
        InitializeComponent();
        Suspending += OnSuspending;
    }

    private static ServiceProvider RegisterServices()
    {
        return new ServiceCollection()
            .AddTransient<AccountsViewModel>()
            .AddTransient<AddAccountViewModel>()
            .AddTransient<OTPCodeViewModel>()
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<ISecretsService, SecretsService>()
            .AddSingleton<INavigationService, NavigationService>(_ =>
                new NavigationService(
                    (Frame)Window.Current.Content,
                    new Dictionary<string, Type>
                    {
                        { nameof(AccountsViewModel), typeof(AccountsPage) },
                        { nameof(AddAccountViewModel), typeof(AddAccountPage) },
                        { nameof(OTPCodeViewModel), typeof(OTPCodePage) }
                    }))
            .BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        ServiceContainer = RegisterServices();

        if (Window.Current.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            rootFrame.NavigationFailed += OnNavigationFailed;

            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                //TODO: Zustand von zuvor angehaltener Anwendung laden
            }

            Window.Current.Content = rootFrame;
        }

        if (e.PrelaunchActivated == false)
        {
            if (rootFrame.Content == null)
                rootFrame.Navigate(typeof(AccountsPage), e.Arguments);

            Window.Current.Activate();
        }
    }

    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e) => throw new Exception("Failed to load Page " + e.SourcePageType.FullName);

    private void OnSuspending(object sender, SuspendingEventArgs e)
    {
        var deferral = e.SuspendingOperation.GetDeferral();
        //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
        deferral.Complete();
    }
}