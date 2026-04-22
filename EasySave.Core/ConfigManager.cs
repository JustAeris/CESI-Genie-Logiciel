using System.Text.Json;

namespace EasySave.Core
{
    public class ConfigManager
    {
        // Singleton
        private static ConfigManager? _instance;
        private static readonly object _lock = new object();

        // Attributs
        private string _configPath = "config.json";
        public List<BackupJob> Jobs { get; private set; } = new List<BackupJob>();

        // private  Constructor 
        private ConfigManager()
        {
            Load();
        }

        // Unique instance
        public static ConfigManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new ConfigManager();
                    return _instance;
                }
            }
        }

        // Load jobs from JSON file
        public void Load()
        {
            if (File.Exists(_configPath))
            {
                string json = File.ReadAllText(_configPath);
                Jobs = JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();
            }
        }

        // Save jobs to JSON file
        public void Save()
        {
            string json = JsonSerializer.Serialize(Jobs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
    }
}