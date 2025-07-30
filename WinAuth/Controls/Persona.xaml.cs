using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WinAuth.Controls;

public sealed partial class Persona : UserControl
{
    public Persona() => InitializeComponent();

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(Persona),
            new PropertyMetadata(null, OnImageSourceChanged));

    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (Persona)d;
        var image = e.NewValue as ImageSource;

        control.AvatarBrush.ImageSource = image;
        control.FallbackIcon.Visibility = image == null ? Visibility.Visible : Visibility.Collapsed;
    }
}