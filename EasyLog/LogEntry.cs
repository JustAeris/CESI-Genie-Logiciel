namespace EasyLog;

public class LogEntry
{
    public string Name { get; set; } = "";
    public string SourcePath { get; set; } = "";
    public string TargetPath { get; set; } = "";
    public long FileSize { get; set; }
    public long TransferTimeMs { get; set; }
    public DateTime Timestamp { get; set; }
}
