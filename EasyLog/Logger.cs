namespace EasyLog;

// Singleton (GoF) — un seul logger par processus.
// Utilise le pattern Strategy (GoF) via ILogSerializer : on peut passer de JSON à XML sans modifier cette classe (OCP).
public class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    public static Logger Instance => _instance.Value;

    // Dossier dans lequel les fichiers de log journaliers sont écrits
    private string _logDir;
    // Sérialiseur actif — JSON par défaut, remplaçable à l'exécution sans redémarrage
    private ILogSerializer _serializer = new JsonLogSerializer();
    // Verrou pour rendre l'opération lire→ajouter→écrire atomique entre les threads
    private readonly object _lock = new();

    // Forwarder optionnel pour l'envoi distant ; null quand la destination est "local"
    private LogForwarder? _forwarder;
    // Destination active : "local" | "remote" | "both"
    private string _logDestination = "local";

    private Logger()
    {
        // Dossier de log par défaut : %AppData%\EasySave\logs
        _logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "logs");
        Directory.CreateDirectory(_logDir);
    }

    // Redirige le dossier de log vers un chemin personnalisé — utilisé dans les tests
    public void SetLogDirectory(string path)
    {
        _logDir = path;
        Directory.CreateDirectory(_logDir);
    }

    // Remplace le forwarder distant ; passer null désactive l'envoi réseau
    public void SetForwarder(LogForwarder? forwarder) => _forwarder = forwarder;

    // Met à jour le mode de destination sans redémarrer le logger
    public void SetLogDestination(string destination) => _logDestination = destination;

    // Injection de dépendance (DIP) — injecte n'importe quel ILogSerializer à l'exécution
    public void SetSerializer(ILogSerializer serializer) => _serializer = serializer;

    // Ajout thread-safe : lit le fichier existant, ajoute l'entrée, réécrit le fichier.
    public void Log(LogEntry entry)
    {
        // Toujours écrire localement — garantit un mode dégradé si le réseau est indisponible
        lock (_lock)
        {
            var filePath = GetLogFilePath();
            List<LogEntry> entries = [];

            // Charge les entrées existantes si le fichier de ce jour existe déjà
            if (File.Exists(filePath))
                entries = _serializer.Deserialize(File.ReadAllText(filePath)).ToList();

            entries.Add(entry);
            File.WriteAllText(filePath, _serializer.Serialize(entries));
        }

        // Transmet au serveur Docker uniquement si la destination est "remote" ou "both"
        if (_forwarder != null && (_logDestination == "remote" || _logDestination == "both"))
            _ = _forwarder.ForwardAsync(entry);
    }

    // Construit le chemin du fichier de log du jour : <logDir>/<yyyy-MM-dd>.json ou .xml
    private string GetLogFilePath() =>
        Path.Combine(_logDir, $"{DateTime.Now:yyyy-MM-dd}{_serializer.FileExtension}");
}
