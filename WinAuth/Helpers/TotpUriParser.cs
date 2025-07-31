using System;
using System.Text.RegularExpressions;
using System.Web;
using WinAuth.Models;

namespace WinAuth.Parsers;

public static class TotpUriParser
{
    public static TotpEntry Parse(string uri)
    {
        if (!uri.StartsWith("otpauth://totp/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Not a valid TOTP URI.");

        var raw = new Uri(uri);
        var label = Uri.UnescapeDataString(raw.AbsolutePath.TrimStart('/')); // e.g. "GitHub:alice@example.com"

        var issuerFromLabel = default(string);
        string account;

        // parse label into issuer:account if present
        var labelMatch = Regex.Match(label, @"^([^:]+):(.*)$");
        if (labelMatch.Success)
        {
            issuerFromLabel = labelMatch.Groups[1].Value;
            account = labelMatch.Groups[2].Value;
        }
        else
        {
            account = label; // fallback: no issuer in label
        }

        // parse query parameters
        var queryParams = HttpUtility.ParseQueryString(raw.Query);

        var secret = queryParams["secret"];
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("TOTP URI must contain a secret.");

        var issuerFromParam = queryParams["issuer"];
        var algorithm = queryParams["algorithm"] ?? "SHA1";
        var digits = int.TryParse(queryParams["digits"], out int d) ? d : 6;
        var period = int.TryParse(queryParams["period"], out int p) ? p : 30;

        // prefer issuer= param, fallback to label if needed
        var issuer = issuerFromParam ?? issuerFromLabel;

        return new TotpEntry
        {
            Issuer = issuer ?? "Unnamed issuer",
            Account = account,
            Secret = secret,
            Digits = digits,
            Period = period,
            Algorithm = algorithm.ToUpperInvariant()
        };
    }
}