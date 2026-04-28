namespace EasyLog;

// Strategy interface (GoF) — decouples serialization format from Logger and StateManager.
// Add a new format by implementing this interface; no existing code needs modification (OCP).
public interface ILogSerializer
{
    string FileExtension { get; } // ".json" or ".xml"
    string Serialize(IEnumerable<LogEntry> entries);
    IEnumerable<LogEntry> Deserialize(string raw);
}
