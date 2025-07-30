using WinAuth.Models;

namespace WinAuth.Messages;

public class UseTotpMessage(TotpEntry entry)
{
    public readonly TotpEntry Entry = entry;
}