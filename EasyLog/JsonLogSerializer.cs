using System.Text.Json;

namespace EasyLog;

// Stratégie concrète (GoF) — sérialisation au format JSON.
public class JsonLogSerializer : ILogSerializer
{
    // Options partagées entre toutes les instances — WriteIndented rend le fichier lisible par un humain
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    // Retourne ".json" pour que Logger nomme correctement le fichier de log du jour
    public string FileExtension => ".json";

    // Sérialise la liste d'entrées en tableau JSON indenté
    public string Serialize(IEnumerable<LogEntry> entries) =>
        JsonSerializer.Serialize(entries.ToList(), _options);

    // Désérialise un tableau JSON en liste d'entrées ; retourne une liste vide si la valeur est null
    public IEnumerable<LogEntry> Deserialize(string raw) =>
        JsonSerializer.Deserialize<List<LogEntry>>(raw) ?? [];
}
