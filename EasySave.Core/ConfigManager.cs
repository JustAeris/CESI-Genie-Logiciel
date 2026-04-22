using System.Text.Json;

namespace EasySave.Core;

public class ConfigManager
{
    private static readonly Lazy<ConfigManager> _instance = new(() => new ConfigManager());
    public static ConfigManager Instance => _instance.Value;

    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private readonly string _configPath;

    public List<BackupJob> Jobs { get; private set; } = [];

    private ConfigManager()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave");
        Directory.CreateDirectory(dir);
        _configPath = Path.Combine(dir, "config.json");
    }

    public void Load()
    {
        if (!File.Exists(_configPath)) return;
        var raw = File.ReadAllText(_configPath);
        Jobs = JsonSerializer.Deserialize<List<BackupJob>>(raw) ?? [];
    }

    public void Save()
    {
        File.WriteAllText(_configPath, JsonSerializer.Serialize(Jobs, _jsonOptions));
    }
}
