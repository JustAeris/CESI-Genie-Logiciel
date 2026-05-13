using System.Text.Json;
using EasyLog;

namespace EasySave.Tests;

[Collection("Singletons")]
public class LogForwarderTests : IDisposable
{
    private readonly string _testDir;

    public LogForwarderTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
        Logger.Instance.SetLogDirectory(_testDir);
        Logger.Instance.SetSerializer(new JsonLogSerializer());
        Logger.Instance.SetLogDestination("local");
        Logger.Instance.SetForwarder(null);
    }

    public void Dispose()
    {
        Logger.Instance.SetForwarder(null);
        Logger.Instance.SetLogDestination("local");
        Logger.Instance.SetSerializer(new JsonLogSerializer());
        Logger.Instance.SetLogDirectory(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "logs"));
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    private static LogEntry MakeEntry(string name = "test") => new()
    {
        Name = name,
        FileSource = @"C:\src\file.txt",
        FileTarget = @"C:\dst\file.txt",
        FileSize = 512,
        FileTransferTime = 10.0,
        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
    };

    // PDF T9: "log local écrit même quand le forward échoue"
    [Fact]
    public void Log_WhenForwardFails_LocalLogIsAlwaysWritten()
    {
        Logger.Instance.SetForwarder(new LogForwarder("http://localhost:1/logs")); // unreachable
        Logger.Instance.SetLogDestination("remote");

        Logger.Instance.Log(MakeEntry());

        Assert.Single(Directory.GetFiles(_testDir, "*.json"));
    }

    [Fact]
    public void Log_WhenDestinationIsBoth_LocalLogWrittenEvenIfForwardFails()
    {
        Logger.Instance.SetForwarder(new LogForwarder("http://localhost:1/logs"));
        Logger.Instance.SetLogDestination("both");

        Logger.Instance.Log(MakeEntry());

        Assert.Single(Directory.GetFiles(_testDir, "*.json"));
    }

    [Fact]
    public void Log_WhenDestinationIsLocal_NoForwarderSet_LocalLogWritten()
    {
        Logger.Instance.SetLogDestination("local");

        Logger.Instance.Log(MakeEntry());

        Assert.Single(Directory.GetFiles(_testDir, "*.json"));
    }

    // Mode dégradé: multiple entries still accumulate locally when forward is unreachable
    [Fact]
    public void Log_WhenForwardFails_MultipleEntriesAccumulate()
    {
        Logger.Instance.SetForwarder(new LogForwarder("http://localhost:1/logs"));
        Logger.Instance.SetLogDestination("remote");

        Logger.Instance.Log(MakeEntry("job1"));
        Logger.Instance.Log(MakeEntry("job2"));

        var file = Directory.GetFiles(_testDir, "*.json").First();
        var entries = JsonSerializer.Deserialize<List<LogEntry>>(File.ReadAllText(file))!;
        Assert.Equal(2, entries.Count);
    }

    [Fact]
    public void LogEntry_MachineField_IsAutoPopulated()
    {
        var entry = MakeEntry();

        Assert.False(string.IsNullOrEmpty(entry.Machine));
        Assert.Equal(Environment.MachineName, entry.Machine);
    }

    [Fact]
    public void LogEntry_MachineField_IsPersistedInLocalLog()
    {
        Logger.Instance.Log(MakeEntry());

        var file = Directory.GetFiles(_testDir, "*.json").First();
        var entries = JsonSerializer.Deserialize<List<LogEntry>>(File.ReadAllText(file))!;

        Assert.Equal(Environment.MachineName, entries[0].Machine);
    }
}
