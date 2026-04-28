using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Core;

// Singleton (GoF) — one config store per process.
// Owns AppConfig and exposes convenience properties so callers don't need to drill into Config.
public class ConfigManager
{
    private static readonly Lazy<ConfigManager> _instance = new(() => new ConfigManager());
    public static ConfigManager Instance => _instance.Value;

    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
    // PropertyNameCaseInsensitive: tolerates casing differences from older config files.
    // JsonStringEnumConverter reads both "Full" (string) and 0 (integer) — v1.0 compat.
    private static readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private string _configPath;

    public AppConfig Config { get; private set; } = new();

    // Convenience properties — avoids drilling into Config at every call site.
    public List<BackupJob> Jobs => Config.Jobs;
    public string LogFormat => Config.LogFormat;

    private ConfigManager()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave");
        Directory.CreateDirectory(dir);
        _configPath = Path.Combine(dir, "config.json");
    }

    // For tests: redirect config file to a temp path.
    public void SetConfigPath(string path) => _configPath = path;

    public void Load()
    {
        if (!File.Exists(_configPath))
        {
            Config = new AppConfig();
            return;
        }

        var raw = File.ReadAllText(_configPath).TrimStart();

        if (raw.StartsWith('['))
        {
            // v1.0 format: plain array of BackupJob — migrate transparently, new fields get defaults.
            var jobs = JsonSerializer.Deserialize<List<BackupJob>>(raw, _readOptions) ?? [];
            Config = new AppConfig { Jobs = jobs };
        }
        else
        {
            // v1.1 format: full AppConfig object.
            Config = JsonSerializer.Deserialize<AppConfig>(raw, _readOptions) ?? new AppConfig();
        }
    }

    public void Save()
    {
        File.WriteAllText(_configPath, JsonSerializer.Serialize(Config, _writeOptions));
    }
}
