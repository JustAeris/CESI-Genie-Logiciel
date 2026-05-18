using EasyLog;

namespace EasySave.Core;

// Singleton (GoF) — un seul gestionnaire d'état par processus.
// Délègue la sérialisation à la Strategy ILogSerializer (GoF) — Persist() ne change pas quand le format change (DIP).
public class StateManager
{
    private static readonly Lazy<StateManager> _instance = new(() => new StateManager());
    public static StateManager Instance => _instance.Value;

    // Verrou pour garantir que lecture, modification et écriture de _states sont atomiques
    private readonly object _lock = new();

    // Dossier où le fichier state.json / state.xml est écrit
    private string _stateDir;

    // Liste des états en mémoire, un par job actif ou terminé
    private List<BackupState> _states = new List<BackupState>();

    // Sérialiseur actif — JSON par défaut, remplacé par le même que Logger via SetSerializer()
    private ILogSerializer _serializer = new JsonLogSerializer();

    private StateManager()
    {
        // Dossier par défaut : %AppData%\EasySave (même racine que la config)
        _stateDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave");
        Directory.CreateDirectory(_stateDir);
    }

    // Redirige le dossier d'état — utilisé dans les tests pour écrire dans un dossier temporaire
    public void SetStateDirectory(string path)
    {
        _stateDir = path;
        Directory.CreateDirectory(_stateDir);
    }

    // Remplace le sérialiseur pour synchroniser le format avec celui de Logger
    public void SetSerializer(ILogSerializer serializer) => _serializer = serializer;

    // Vide la liste des états en mémoire — appelé entre les tests pour repartir d'un état propre
    public void ClearStates()
    {
        lock (_lock) _states = new List<BackupState>();
    }

    // Met à jour l'état d'un job (ou l'ajoute s'il n'existe pas encore), puis persiste sur le disque.
    public void Update(BackupState state)
    {
        lock (_lock)
        {
            // Supprime l'ancien état de ce job avant d'ajouter le nouveau — évite les doublons
            _states.RemoveAll(s => s.Name == state.Name);
            _states.Add(state);
            Persist();
        }
    }

    // Retourne une copie de la liste des états pour éviter les modifications concurrentes depuis l'extérieur
    public List<BackupState> GetAll()
    {
        lock (_lock) return _states.ToList();
    }

    // Écrit la liste des états dans state.json ou state.xml selon le sérialiseur actif
    private void Persist()
    {
        var path = Path.Combine(_stateDir, $"state{_serializer.FileExtension}");
        File.WriteAllText(path, SerializeStates(_states));
    }

    // Choisit la méthode de sérialisation selon le format actif
    private string SerializeStates(List<BackupState> states)
    {
        if (_serializer.FileExtension == ".xml")
            return SerializeXml(states);

        // JSON par défaut — indenté pour lisibilité
        return System.Text.Json.JsonSerializer.Serialize(
            states,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
        );
    }

    // Construit un document XML avec une racine <States> contenant un <BackupState> par job
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
