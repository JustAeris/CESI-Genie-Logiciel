using System.Text.Json;

namespace EasyLog;

public class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    public static Logger Instance => _instance.Value;

    private string _logDir;
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private Logger()
    {
        _logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "logs");
        Directory.CreateDirectory(_logDir);
    }

    public void SetLogDirectory(string path)
    {
        _logDir = path;
        Directory.CreateDirectory(_logDir);
    }

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

    private string GetLogFilePath() =>
        Path.Combine(_logDir, $"{DateTime.Now:yyyy-MM-dd}.json");

    private void WriteToFile(string json) =>
        File.WriteAllText(GetLogFilePath(), json);
}
