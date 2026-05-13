namespace EasyLog;

// Singleton (GoF) — one logger per process.
// Uses the Strategy (GoF) pattern via ILogSerializer: swap JSON/XML without touching this class (OCP).
public class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    public static Logger Instance => _instance.Value;

    private string _logDir;
    private ILogSerializer _serializer = new JsonLogSerializer(); // default: v1.0 behaviour
    private readonly object _lock = new();

    private LogForwarder? _forwarder;
    private string _logDestination = "local"; // "local" | "remote" | "both"

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

    public void SetForwarder(LogForwarder? forwarder) => _forwarder = forwarder;

    public void SetLogDestination(string destination) => _logDestination = destination;

    // Dependency injection (DIP) — inject any ILogSerializer at runtime.
    public void SetSerializer(ILogSerializer serializer) => _serializer = serializer;

    // Thread-safe append: read → append → write.
    public void Log(LogEntry entry)
    {
        // Always write locally — guarantees mode dégradé if remote is unreachable
        lock (_lock)
        {
            var filePath = GetLogFilePath();
            List<LogEntry> entries = [];

            if (File.Exists(filePath))
                entries = _serializer.Deserialize(File.ReadAllText(filePath)).ToList();

            entries.Add(entry);
            File.WriteAllText(filePath, _serializer.Serialize(entries));
        }

        // Forward to Docker log server only when destination is remote or both
        if (_forwarder != null && (_logDestination == "remote" || _logDestination == "both"))
            _ = _forwarder.ForwardAsync(entry);
    }

    private string GetLogFilePath() =>
        Path.Combine(_logDir, $"{DateTime.Now:yyyy-MM-dd}{_serializer.FileExtension}");
}
