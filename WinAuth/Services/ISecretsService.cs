using System.Collections.Generic;
using WinAuth.Models;

namespace WinAuth.Services
{
    public interface ISecretsService
    {
        void DeleteEntry(string id);
        List<TotpEntry> LoadAllEntries();
        void SaveEntry(TotpEntry entry);
    }
}