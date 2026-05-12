namespace EasyLog;

/// <summary>
/// Forwards log entries to a remote Docker log server via HTTP.
/// Fire-and-forget: does not block the backup process.
/// Falls back gracefully if the server is unreachable.
/// </summary>
public class LogForwarder
{
    private static readonly HttpClient _httpClient = new();
    private readonly string _serverUrl;
    private readonly string _machineName = Environment.MachineName;

    public LogForwarder(string serverUrl)
    {
        _serverUrl = serverUrl;
    }

    public async Task ForwardAsync(LogEntry entry)
    {
        try
        {
            var payload = new
            {
                Machine = _machineName,
                entry.Name,
                entry.FileSource,
                entry.FileTarget,
                entry.FileSize,
                entry.FileTransferTime,
                entry.EncryptionTime,
                entry.Timestamp
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await _httpClient.PostAsync(_serverUrl, content, cts.Token);
        }
        catch
        {
            // Silently ignore — local log is always written regardless
        }
    }
}