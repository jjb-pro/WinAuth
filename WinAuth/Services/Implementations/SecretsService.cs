using System.Collections.Generic;
using System.Text.Json;
using WinAuth.Models;
using Windows.Security.Credentials;

namespace WinAuth.Services.Implementations;

public class SecretsService : ISecretsService
{
    private const string ResourceName = "WinAuth";
    private readonly PasswordVault _vault = new();

    public void SaveEntry(TotpEntry entry)
    {
        var id = $"{entry.Issuer}:{entry.Account}";
        RemoveEntry(id);

        string json = SerializeTotpEntry(entry);
        _vault.Add(new PasswordCredential(ResourceName, id, json));
    }

    public List<TotpEntry> LoadAllEntries()
    {
        var entries = new List<TotpEntry>();

        try
        {
            var credentials = _vault.FindAllByResource(ResourceName);
            foreach (var cred in credentials)
            {
                cred.RetrievePassword();
                var entry = DeserializeTotpEntry(cred.Password);
                entries.Add(entry);
            }
        }
        catch { }

        return entries;
    }

    public void RemoveEntry(string id)
    {
        try
        {
            var cred = _vault.Retrieve(ResourceName, id);
            _vault.Remove(cred);
        }
        catch { }
    }

    private static string SerializeTotpEntry(TotpEntry entry) => JsonSerializer.Serialize(entry);

    private static TotpEntry DeserializeTotpEntry(string json) => JsonSerializer.Deserialize<TotpEntry>(json);
}