using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Core;

// Singleton (GoF) — un seul gestionnaire de configuration par processus.
// Détient AppConfig en mémoire et expose des propriétés raccourcies pour éviter de descendre dans Config à chaque appel.
public class ConfigManager
{
    private static readonly Lazy<ConfigManager> _instance = new(() => new ConfigManager());
    public static ConfigManager Instance => _instance.Value;

    // Options d'écriture — JSON indenté pour que le fichier reste lisible par un humain
    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    // PropertyNameCaseInsensitive : tolère les différences de casse entre anciennes et nouvelles versions du fichier.
    // JsonStringEnumConverter : lit "Full" (chaîne) et 0 (entier) — compatibilité avec le format v1.0.
    private static readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    // Chemin complet vers config.json sur le disque, ex. %AppData%\EasySave\config.json
    private string _configPath;

    // Objet de configuration en mémoire ; modifié directement, puis persisté via Save()
    public AppConfig Config { get; private set; } = new();

    // Propriétés raccourcies — évitent de taper ConfigManager.Instance.Config.X à chaque appel
    public List<BackupJob> Jobs => Config.Jobs;
    public string LogFormat => Config.LogFormat;

    private ConfigManager()
    {
        // Dossier de stockage dans AppData roaming pour persister entre les sessions utilisateur
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave");
        Directory.CreateDirectory(dir);
        _configPath = Path.Combine(dir, "config.json");
    }

    // Redirige le chemin du fichier de config — utilisé dans les tests pour isoler l'écriture sur disque
    public void SetConfigPath(string path) => _configPath = path;

    // Lit le fichier config.json et peuple Config.
    // Gère deux formats historiques : v1.0 (tableau JSON brut) et v1.1+ (objet AppConfig complet).
    public void Load()
    {
        // Pas de fichier encore — on garde l'AppConfig par défaut avec une liste de jobs vide
        if (!File.Exists(_configPath))
        {
            Config = new AppConfig();
            return;
        }

        var raw = File.ReadAllText(_configPath).TrimStart();

        if (raw.StartsWith('['))
        {
            // Format v1.0 : tableau brut de BackupJob — migration transparente, les nouveaux champs reçoivent leurs valeurs par défaut
            var jobs = JsonSerializer.Deserialize<List<BackupJob>>(raw, _readOptions) ?? [];
            Config = new AppConfig { Jobs = jobs };
        }
        else
        {
            // Format v1.1+ : objet AppConfig complet
            Config = JsonSerializer.Deserialize<AppConfig>(raw, _readOptions) ?? new AppConfig();
        }
    }

    // Sérialise Config sur le disque — appelé après chaque modification de paramètre pour une persistance immédiate
    public void Save()
    {
        File.WriteAllText(_configPath, JsonSerializer.Serialize(Config, _writeOptions));
    }
}
