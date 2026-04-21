using System.Text.Json.Serialization;

namespace EasyLog;

public class LogEntry
{
    public string Name { get; set; } = "";
    public string FileSource { get; set; } = "";
    public string FileTarget { get; set; } = "";
    public long FileSize { get; set; }
    public double FileTransferTime { get; set; }

    [JsonPropertyName("time")]
    public string Timestamp { get; set; } = "";
}
