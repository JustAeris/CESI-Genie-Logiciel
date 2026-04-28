using EasyLog;
using EasySave.Core;

namespace EasySave.Tests;

[Collection("Singletons")]
public class StateManagerTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _defaultAppDataDir;

    public StateManagerTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);

        _defaultAppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave");

        StateManager.Instance.SetStateDirectory(_testDir);
        StateManager.Instance.SetSerializer(new JsonLogSerializer());
        StateManager.Instance.ClearStates();
    }

    public void Dispose()
    {
        StateManager.Instance.ClearStates();
        StateManager.Instance.SetSerializer(new JsonLogSerializer());
        StateManager.Instance.SetStateDirectory(_defaultAppDataDir);
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    private static BackupState MakeState(string name, int total = 5, int left = 3) => new()
    {
        Name = name,
        State = "Active",
        TotalFilesToCopy = total,
        TotalFilesSize = 1024,
        NbFilesLeftToDo = left,
        SizeLeft = 512,
        Progression = 40.0,
        SourceFilePath = @"C:\src\file.txt",
        TargetFilePath = @"C:\dst\file.txt",
        Timestamp = DateTime.Now
    };

    // --- File location ---

    [Fact]
    public void Update_CreatesStateFile_InConfiguredDirectory()
    {
        StateManager.Instance.Update(MakeState("job1"));

        var files = Directory.GetFiles(_testDir, "state.*");
        Assert.Single(files);
    }

    [Fact]
    public void Update_Json_CreatesJsonFile()
    {
        StateManager.Instance.SetSerializer(new JsonLogSerializer());
        StateManager.Instance.Update(MakeState("job1"));

        Assert.True(File.Exists(Path.Combine(_testDir, "state.json")));
    }

    [Fact]
    public void Update_Xml_CreatesXmlFile()
    {
        StateManager.Instance.SetSerializer(new XmlLogSerializer());
        StateManager.Instance.Update(MakeState("job1"));

        Assert.True(File.Exists(Path.Combine(_testDir, "state.xml")));
    }

    // --- JSON content ---

    [Fact]
    public void Update_Json_StatePersistedCorrectly()
    {
        StateManager.Instance.SetSerializer(new JsonLogSerializer());
        var state = MakeState("myJob");
        StateManager.Instance.Update(state);

        var raw = File.ReadAllText(Path.Combine(_testDir, "state.json"));
        Assert.Contains("myJob", raw);
        Assert.Contains("Active", raw);
    }

    // --- XML content ---

    [Fact]
    public void Update_Xml_ProducesValidXml()
    {
        StateManager.Instance.SetSerializer(new XmlLogSerializer());
        StateManager.Instance.Update(MakeState("xmlJob"));

        var raw = File.ReadAllText(Path.Combine(_testDir, "state.xml"));
        var doc = System.Xml.Linq.XDocument.Parse(raw);

        Assert.Equal("States", doc.Root?.Name.LocalName);
        Assert.Single(doc.Root!.Elements("BackupState"));
        Assert.Equal("xmlJob", (string?)doc.Root.Element("BackupState")?.Element("Name"));
    }

    // --- Update replaces existing state by name ---

    [Fact]
    public void Update_SameJobName_ReplacesEntry()
    {
        StateManager.Instance.Update(MakeState("job", left: 5));
        StateManager.Instance.Update(MakeState("job", left: 2));

        var states = StateManager.Instance.GetAll();
        Assert.Single(states);
        Assert.Equal(2, states[0].NbFilesLeftToDo);
    }

    // --- GetAll ---

    [Fact]
    public void GetAll_ReturnsAllUpdatedStates()
    {
        StateManager.Instance.Update(MakeState("a"));
        StateManager.Instance.Update(MakeState("b"));
        StateManager.Instance.Update(MakeState("c"));

        Assert.Equal(3, StateManager.Instance.GetAll().Count);
    }
}
