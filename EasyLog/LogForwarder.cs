namespace EasyLog;

// Envoie une entrée de log vers un serveur Docker distant via HTTP POST.
// Utilisé quand LogDestination est "remote" ou "both".
public class LogForwarder
{
    // Instance partagée entre toutes les instances — évite de créer une nouvelle socket à chaque requête
    private static readonly HttpClient _httpClient = new();
    // URL du serveur distant, configurée dans AppConfig.LogServerUrl
    private readonly string _serverUrl;

    public LogForwarder(string serverUrl)
    {
        _serverUrl = serverUrl;
    }

    // Envoie l'entrée en JSON de façon asynchrone (fire-and-forget).
    // Les erreurs sont silencieuses : une panne réseau ne doit jamais bloquer ou faire planter un job.
    public async Task ForwardAsync(LogEntry entry)
    {
        try
        {
            // Sérialise l'entrée en JSON et prépare le corps de la requête
            var json = System.Text.Json.JsonSerializer.Serialize(entry);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Timeout de 2 secondes — évite de bloquer si le serveur est inaccessible
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await _httpClient.PostAsync(_serverUrl, content, cts.Token);
        }
        catch
        {
            // Silently ignore — le log local est toujours écrit, peu importe l'état du réseau
        }
    }
}
