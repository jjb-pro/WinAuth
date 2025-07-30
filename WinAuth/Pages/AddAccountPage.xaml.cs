using Microsoft.Extensions.DependencyInjection;
using WinAuth.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinAuth.Pages;

public sealed partial class AddAccountPage : Page
{
    public AddAccountViewModel ViewModel { get; } = ((App)Application.Current).ServiceContainer.GetRequiredService<AddAccountViewModel>();

    public AddAccountPage() => InitializeComponent();
}