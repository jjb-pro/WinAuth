using System;
using System.Security.Cryptography;
using WinAuth.Models;

namespace WinAuth.Helpers;

public static class TotpHelper
{
    public static string GenerateTOTP(TotpEntry entry)
    {
        var key = Base32Helper.Decode(entry.Secret);
        var timestep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / entry.Period;
        var timestepBytes = BitConverter.GetBytes(timestep);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timestepBytes);

        using HMAC hmac = entry.Algorithm.ToUpperInvariant() switch
        {
            "SHA256" => new HMACSHA256(key),
            "SHA512" => new HMACSHA512(key),
            _ => new HMACSHA1(key),
        };

        var hash = hmac.ComputeHash(timestepBytes);
        var offset = hash[hash.Length - 1] & 0x0F;

        var binaryCode = ((hash[offset] & 0x7f) << 24)
                       | ((hash[offset + 1] & 0xff) << 16)
                       | ((hash[offset + 2] & 0xff) << 8)
                       | (hash[offset + 3] & 0xff);

        var otp = binaryCode % (int)Math.Pow(10, entry.Digits);
        return otp.ToString($"D{entry.Digits}");
    }
}