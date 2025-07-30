using WinAuth.Models;

namespace WinAuth.Messages;

public class NewTotpMessage(TotpEntry entry)
{
    public readonly TotpEntry Entry = entry;
}