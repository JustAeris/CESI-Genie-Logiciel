namespace EasyLog;

// Strategy interface (GoF) — découple le format de sérialisation de Logger et StateManager.
// Pour ajouter un nouveau format, implémenter cette interface sans modifier le code existant (OCP).
public interface ILogSerializer
{
    // Extension de fichier utilisée pour nommer les fichiers de log, ex. ".json" ou ".xml"
    string FileExtension { get; }
    // Convertit une liste d'entrées de log en chaîne prête à écrire sur disque
    string Serialize(IEnumerable<LogEntry> entries);
    // Relit le contenu brut d'un fichier et le reconvertit en liste d'entrées
    IEnumerable<LogEntry> Deserialize(string raw);
}
