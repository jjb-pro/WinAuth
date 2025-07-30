using Microsoft.Extensions.DependencyInjection;
using WinAuth.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinAuth.Pages;

public sealed partial class AccountsPage : Page
{
    public AccountsViewModel ViewModel { get; } = ((App)Application.Current).ServiceContainer.GetRequiredService<AccountsViewModel>();

    public AccountsPage() => InitializeComponent();
}