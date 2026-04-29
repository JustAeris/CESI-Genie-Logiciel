using EasyLog;

namespace EasySave.Core;

// Singleton (GoF) — one state store per process.
// Delegates serialization to ILogSerializer Strategy (GoF) — DIP respected, Persist() unchanged when format changes.
public class StateManager
{
    private static readonly Lazy<StateManager> _instance = new(() => new StateManager());
    public static StateManager Instance => _instance.Value;

    private string _stateDir;
    private List<BackupState> _states = new List<BackupState>();
    private ILogSerializer _serializer = new JsonLogSerializer();

    private StateManager()
    {
        _stateDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave");
        Directory.CreateDirectory(_stateDir);
    }

    public void SetStateDirectory(string path)
    {
        _stateDir = path;
        Directory.CreateDirectory(_stateDir);
    }

    public void SetSerializer(ILogSerializer serializer) => _serializer = serializer;

    public void ClearStates() => _states = new List<BackupState>();

    public void Update(BackupState state)
    {
        _states.RemoveAll(s => s.Name == state.Name);
        _states.Add(state);
        Persist();
    }

    public List<BackupState> GetAll() => _states;

    private void Persist()
    {
        var path = Path.Combine(_stateDir, $"state{_serializer.FileExtension}");
        File.WriteAllText(path, SerializeStates(_states));
    }

    private string SerializeStates(List<BackupState> states)
    {
        if (_serializer.FileExtension == ".xml")
            return SerializeXml(states);

        return System.Text.Json.JsonSerializer.Serialize(
            states,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
        );
    }

    private static string SerializeXml(List<BackupState> states)
    {
        var doc = new System.Xml.Linq.XDocument(
            new System.Xml.Linq.XDeclaration("1.0", "utf-8", null),
            new System.Xml.Linq.XElement("States",
                states.Select(s => new System.Xml.Linq.XElement("BackupState",
                    new System.Xml.Linq.XElement("Name", s.Name),
                    new System.Xml.Linq.XElement("SourceFilePath", s.SourceFilePath),
                    new System.Xml.Linq.XElement("TargetFilePath", s.TargetFilePath),
                    new System.Xml.Linq.XElement("State", s.State),
                    new System.Xml.Linq.XElement("TotalFilesToCopy", s.TotalFilesToCopy),
                    new System.Xml.Linq.XElement("TotalFilesSize", s.TotalFilesSize),
                    new System.Xml.Linq.XElement("NbFilesLeftToDo", s.NbFilesLeftToDo),
                    new System.Xml.Linq.XElement("SizeLeft", s.SizeLeft),
                    new System.Xml.Linq.XElement("Progression", s.Progression),
                    new System.Xml.Linq.XElement("Timestamp", s.Timestamp)
                ))
            )
        );
        return doc.ToString();
    }
}