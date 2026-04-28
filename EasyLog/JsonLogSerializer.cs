using System.Text.Json;

namespace EasyLog;

// Concrete Strategy (GoF) for JSON serialization.
public class JsonLogSerializer : ILogSerializer
{
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public string FileExtension => ".json";

    public string Serialize(IEnumerable<LogEntry> entries) =>
        JsonSerializer.Serialize(entries.ToList(), _options);

    public IEnumerable<LogEntry> Deserialize(string raw) =>
        JsonSerializer.Deserialize<List<LogEntry>>(raw) ?? [];
}
