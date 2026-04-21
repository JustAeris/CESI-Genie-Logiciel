namespace EasyLog;

public class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    public static Logger Instance => _instance.Value;

    public void Log(LogEntry entry) { }
}
