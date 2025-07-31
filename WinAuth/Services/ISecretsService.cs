using System.Collections.Generic;
using WinAuth.Models;

namespace WinAuth.Services
{
    public interface ISecretsService
    {
        void RemoveEntry(string id);
        List<TotpEntry> LoadAllEntries();
        void SaveEntry(TotpEntry entry);
    }
}