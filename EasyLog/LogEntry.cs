using System.Text.Json.Serialization;

namespace EasyLog;

/// <summary>
/// Represents a single file-transfer record written to the daily JSON log.
/// </summary>
public class LogEntry
{
    /// <summary>Hostname of the machine that produced this entry.</summary>
    public string Machine { get; set; } = Environment.MachineName;

    /// <summary>Name of the backup job that produced this entry.</summary>
    public string Name { get; set; } = "";

    /// <summary>Absolute path of the source file.</summary>
    public string FileSource { get; set; } = "";

    /// <summary>Absolute path of the destination file.</summary>
    public string FileTarget { get; set; } = "";

    /// <summary>Size of the transferred file in bytes.</summary>
    public long FileSize { get; set; }

    /// <summary>Time taken to transfer the file, in milliseconds.</summary>
    public double FileTransferTime { get; set; }

    /// <summary>Timestamp of the transfer (format: yyyy-MM-dd HH:mm:ss).</summary>
    [JsonPropertyName("time")]
    public string Timestamp { get; set; } = "";

    /// <summary>Encryption duration in ms. 0 = no encryption, &gt;0 = success, &lt;0 = error.</summary>
    public long EncryptionTime { get; set; }
}
