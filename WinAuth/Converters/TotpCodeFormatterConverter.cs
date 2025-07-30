using System;
using Windows.UI.Xaml.Data;

namespace WinAuth.Converters;

public class TotpCodeFormatterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var code = value as string;
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        if (code.Length == 6)
            return $"{code.Substring(0, 3)} {code.Substring(3)}";
        else if (code.Length == 8)
            return $"{code.Substring(0, 4)} {code.Substring(4)}";
        else
            return code;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}