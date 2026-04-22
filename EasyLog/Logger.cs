using System.Text.Json;

namespace EasyLog;

/// <summary>
/// Singleton logger that appends <see cref="LogEntry"/> records to a daily JSON file.
/// </summary>
public class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());

    /// <summary>Gets the singleton instance of <see cref="Logger"/>.</summary>
    public static Logger Instance => _instance.Value;

    private string _logDir;
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Initializes the logger and creates the default log directory under AppData.
    /// </summary>
    private Logger()
    {
        _logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "logs");
        Directory.CreateDirectory(_logDir);
    }

    /// <summary>
    /// Overrides the log directory (useful for tests or custom deployments).
    /// </summary>
    /// <param name="path">Absolute path to the desired log directory.</param>
    public void SetLogDirectory(string path)
    {
        _logDir = path;
        Directory.CreateDirectory(_logDir);
    }

    /// <summary>
    /// Appends <paramref name="entry"/> to today's JSON log file.
    /// Thread-safe: concurrent calls are serialized via a lock.
    /// </summary>
    /// <param name="entry">The log entry to persist.</param>
    public void Log(LogEntry entry)
    {
        lock (_lock)
        {
            var filePath = GetLogFilePath();
            List<LogEntry> entries = [];

            if (File.Exists(filePath))
            {
                var raw = File.ReadAllText(filePath);
                entries = JsonSerializer.Deserialize<List<LogEntry>>(raw) ?? [];
            }

            entries.Add(entry);
            WriteToFile(JsonSerializer.Serialize(entries, _jsonOptions));
        }
    }

    /// <summary>Returns the path of today's log file (format: yyyy-MM-dd.json).</summary>
    private string GetLogFilePath() =>
        Path.Combine(_logDir, $"{DateTime.Now:yyyy-MM-dd}.json");

    /// <summary>Writes <paramref name="json"/> to today's log file, overwriting existing content.</summary>
    private void WriteToFile(string json) =>
        File.WriteAllText(GetLogFilePath(), json);
namespace EasyLog;

public class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    public static Logger Instance => _instance.Value;

    public void Log(LogEntry entry) { }
}
