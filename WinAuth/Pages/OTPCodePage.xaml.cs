using Microsoft.Extensions.DependencyInjection;
using WinAuth.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinAuth.Pages;

public sealed partial class OTPCodePage : Page
{
    public OTPCodeViewModel ViewModel { get; } = ((App)Application.Current).ServiceContainer.GetRequiredService<OTPCodeViewModel>();

    public OTPCodePage() => InitializeComponent();
}