namespace EasyLog;

public class LogForwarder
{
    private static readonly HttpClient _httpClient = new();
    private readonly string _serverUrl;

    public LogForwarder(string serverUrl)
    {
        _serverUrl = serverUrl;
    }

    public async Task ForwardAsync(LogEntry entry)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(entry);
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
