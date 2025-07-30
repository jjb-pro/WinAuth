
namespace WinAuth.Models;

public class TotpEntry
{
    public string Issuer { get; set; }
    public string Account { get; set; }
    public string Secret { get; set; }
    public int Digits { get; set; } = 6;
    public int Period { get; set; } = 30;
    public string Algorithm { get; set; } = "SHA1";

    public string Id => $"{Issuer}:{Account}";
}