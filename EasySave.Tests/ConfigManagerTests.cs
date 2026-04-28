using System.Text.Json;
using EasySave.Core;

namespace EasySave.Tests;

[Collection("Singletons")]
public class ConfigManagerTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _configFile;
    private readonly string _defaultAppDataPath;

    public ConfigManagerTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
        _configFile = Path.Combine(_testDir, "config.json");

        _defaultAppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "config.json");

        ConfigManager.Instance.SetConfigPath(_configFile);
    }

    public void Dispose()
    {
        ConfigManager.Instance.SetConfigPath(_defaultAppDataPath);
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    // --- Missing file: defaults ---

    [Fact]
    public void Load_FileAbsent_DefaultsApplied()
    {
        ConfigManager.Instance.Load();

        Assert.Empty(ConfigManager.Instance.Jobs);
        Assert.Equal("json", ConfigManager.Instance.LogFormat);
    }

    // --- v1.0 migration: plain array of BackupJob ---

    [Fact]
    public void Load_V1Format_MigratesWithoutError()
    {
        var v1Jobs = new[]
        {
            new { Name = "job1", SourceDir = @"C:\src", TargetDir = @"C:\dst", Type = "Full" }
        };
        File.WriteAllText(_configFile, JsonSerializer.Serialize(v1Jobs));

        ConfigManager.Instance.Load();

        Assert.Single(ConfigManager.Instance.Jobs);
        Assert.Equal("job1", ConfigManager.Instance.Jobs[0].Name);
    }

    [Fact]
    public void Load_V1Format_NewFieldsGetDefaults()
    {
        var v1Jobs = new[] { new { Name = "j", SourceDir = @"C:\s", TargetDir = @"C:\d", Type = "Full" } };
        File.WriteAllText(_configFile, JsonSerializer.Serialize(v1Jobs));

        ConfigManager.Instance.Load();

        Assert.Equal("json", ConfigManager.Instance.LogFormat);
    }

    // --- v1.1 format: full AppConfig ---

    [Fact]
    public void Load_V1_1Format_AllFieldsRead()
    {
        var config = new { Jobs = new[] { new { Name = "j2", SourceDir = @"C:\s", TargetDir = @"C:\d", Type = "Differential" } }, LogFormat = "xml" };
        File.WriteAllText(_configFile, JsonSerializer.Serialize(config));

        ConfigManager.Instance.Load();

        Assert.Single(ConfigManager.Instance.Jobs);
        Assert.Equal("xml", ConfigManager.Instance.LogFormat);
    }

    // --- Save + reload round-trip ---

    [Fact]
    public void Save_ThenLoad_PreservesAllFields()
    {
        ConfigManager.Instance.Load(); // start from defaults
        ConfigManager.Instance.Jobs.Add(new BackupJob { Name = "rt", SourceDir = @"C:\s", TargetDir = @"C:\t", Type = BackupType.Full });
        ConfigManager.Instance.Config.LogFormat = "xml";
        ConfigManager.Instance.Save();

        ConfigManager.Instance.Load();

        Assert.Single(ConfigManager.Instance.Jobs);
        Assert.Equal("xml", ConfigManager.Instance.LogFormat);
        Assert.Equal("rt", ConfigManager.Instance.Jobs[0].Name);
    }

    [Fact]
    public void Save_WritesValidJson()
    {
        ConfigManager.Instance.Load();
        ConfigManager.Instance.Config.LogFormat = "xml";
        ConfigManager.Instance.Save();

        var raw = File.ReadAllText(_configFile);
        var parsed = JsonSerializer.Deserialize<AppConfig>(raw);
        Assert.NotNull(parsed);
    }
}
