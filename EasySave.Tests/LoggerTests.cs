using System.Text.Json;
using System.Xml.Linq;
using EasyLog;

namespace EasySave.Tests;

[Collection("Singletons")]
public class LoggerTests : IDisposable
{
    private readonly string _testDir;

    public LoggerTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
        Logger.Instance.SetLogDirectory(_testDir);
        Logger.Instance.SetSerializer(new JsonLogSerializer()); // always start with JSON (v1.0 default)
    }

    public void Dispose()
    {
        // Reset Logger state BEFORE deleting temp dir so concurrent tests never hit a deleted path.
        Logger.Instance.SetSerializer(new JsonLogSerializer());
        Logger.Instance.SetForwarder(null);
        Logger.Instance.SetLogDestination("local");
        Logger.Instance.SetLogDirectory(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "logs"));
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    private static LogEntry MakeEntry(string name = "test") => new()
    {
        Name = name,
        FileSource = @"\\server\src\file.txt",
        FileTarget = @"\\server\dst\file.txt",
        FileSize = 1024,
        FileTransferTime = 42.5,
        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
    };

    // --- JSON (default / v1.0 retrocompat) ---

    [Fact]
    public void Log_CreatesFileOnFirstCall()
    {
        Logger.Instance.Log(MakeEntry());

        var files = Directory.GetFiles(_testDir, "*.json");
        Assert.Single(files);
    }

    [Fact]
    public void Log_AppendsMultipleEntries()
    {
        Logger.Instance.Log(MakeEntry("job1"));
        Logger.Instance.Log(MakeEntry("job2"));
        Logger.Instance.Log(MakeEntry("job3"));

        var file = Directory.GetFiles(_testDir, "*.json").First();
        var json = File.ReadAllText(file);
        var entries = JsonSerializer.Deserialize<List<LogEntry>>(json);

        Assert.NotNull(entries);
        Assert.Equal(3, entries.Count);
        Assert.Equal("job1", entries[0].Name);
        Assert.Equal("job3", entries[2].Name);
    }

    [Fact]
    public void Log_FileNameContainsTodaysDate()
    {
        Logger.Instance.Log(MakeEntry());

        var file = Directory.GetFiles(_testDir, "*.json").First();
        Assert.Contains(DateTime.Now.ToString("yyyy-MM-dd"), Path.GetFileName(file));
    }

    [Fact]
    public void Log_EntryFieldsArePersisted()
    {
        var entry = MakeEntry("backup-1");
        entry.FileSize = 2048;
        entry.FileTransferTime = 99.9;
        Logger.Instance.Log(entry);

        var file = Directory.GetFiles(_testDir, "*.json").First();
        var entries = JsonSerializer.Deserialize<List<LogEntry>>(File.ReadAllText(file))!;

        Assert.Equal(2048, entries[0].FileSize);
        Assert.Equal(99.9, entries[0].FileTransferTime);
        Assert.Equal("backup-1", entries[0].Name);
    }

    [Fact]
    public void Log_TimestampFieldSerializedAsTime()
    {
        Logger.Instance.Log(MakeEntry());

        var file = Directory.GetFiles(_testDir, "*.json").First();
        var raw = File.ReadAllText(file);

        // [JsonPropertyName("time")] means JSON key is "time" not "Timestamp"
        Assert.Contains("\"time\"", raw);
    }

    [Fact]
    public void SetLogDirectory_ChangesOutputFolder()
    {
        var newDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(newDir);
        Logger.Instance.SetLogDirectory(newDir);

        Logger.Instance.Log(MakeEntry());

        Assert.True(Directory.GetFiles(newDir, "*.json").Length > 0);

        Directory.Delete(newDir, recursive: true);
        Logger.Instance.SetLogDirectory(_testDir);
    }

    // --- XML serializer (v1.1 feature) ---

    [Fact]
    public void SetSerializer_Xml_CreatesXmlFile()
    {
        Logger.Instance.SetSerializer(new XmlLogSerializer());
        Logger.Instance.Log(MakeEntry("xml-job"));

        var files = Directory.GetFiles(_testDir, "*.xml");
        Assert.Single(files);
    }

    [Fact]
    public void SetSerializer_Xml_ProducesValidXml()
    {
        Logger.Instance.SetSerializer(new XmlLogSerializer());
        Logger.Instance.Log(MakeEntry("xml-job"));

        var file = Directory.GetFiles(_testDir, "*.xml").First();
        var doc = XDocument.Load(file);

        Assert.Equal("Logs", doc.Root?.Name.LocalName);
        Assert.Single(doc.Root!.Elements("LogEntry"));
        Assert.Equal("xml-job", (string?)doc.Root.Element("LogEntry")?.Element("Name"));
    }

    [Fact]
    public void SetSerializer_Xml_AppendsTwice_TwoEntries()
    {
        Logger.Instance.SetSerializer(new XmlLogSerializer());
        Logger.Instance.Log(MakeEntry("a"));
        Logger.Instance.Log(MakeEntry("b"));

        var file = Directory.GetFiles(_testDir, "*.xml").First();
        var doc = XDocument.Load(file);

        Assert.Equal(2, doc.Root!.Elements("LogEntry").Count());
    }

    [Fact]
    public void SetSerializer_Xml_EncryptionTimePersistedCorrectly()
    {
        Logger.Instance.SetSerializer(new XmlLogSerializer());

        var entry = MakeEntry("enc-job");
        entry.EncryptionTime = 150;
        Logger.Instance.Log(entry);

        var file = Directory.GetFiles(_testDir, "*.xml").First();
        var doc = XDocument.Load(file);
        var enc = (long?)doc.Root!.Element("LogEntry")?.Element("EncryptionTime");

        Assert.Equal(150L, enc);
    }
}
