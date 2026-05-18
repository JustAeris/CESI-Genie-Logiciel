namespace EasySave.Core;

// Objet de configuration racine persisté dans %AppData%\EasySave\config.json.
// Chargé une seule fois au démarrage par ConfigManager ; modifié en mémoire puis re-sauvegardé à chaque changement de paramètre.
public class AppConfig
{
    // Liste de tous les jobs de sauvegarde configurés (nom, source, destination, type)
    public List<BackupJob> Jobs { get; set; } = [];

    // Format de sérialisation des logs : "json" | "xml"
    public string LogFormat { get; set; } = "json";

    // Extensions transférées avant tout autre fichier, ex. [".pdf", ".docx"] — priorité réseau (T8)
    public List<string> PriorityExtensions { get; set; } = [];

    // Extensions dont les fichiers copiés sont chiffrés par CryptoSoft après transfert ; vide = chiffrer tout
    public List<string> EncryptedExtensions { get; set; } = [];

    // Seuil en Ko au-delà duquel un fichier est considéré "volumineux" et sérialisé entre jobs (T7) ; 0 = désactivé
    public int LargeFileSizeKb { get; set; } = 0;

    // Destination des logs : "local" | "remote" | "both"
    public string LogDestination { get; set; } = "local";

    // URL HTTP du serveur de logs Docker distant
    public string LogServerUrl { get; set; } = "http://localhost:5000/logs";

    // Nom du logiciel métier (ex. "notepad") dont la présence met en pause tous les jobs actifs (T6)
    public string BusinessSoftwareName { get; set; } = "";
}
