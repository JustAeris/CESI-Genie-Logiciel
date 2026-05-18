using System.Text.Json.Serialization;

namespace EasyLog;

// Représente l'enregistrement d'un transfert de fichier écrit dans le log journalier.
// Une entrée est créée par fichier copié, qu'il y ait eu chiffrement ou non.
public class LogEntry
{
    // Nom de la machine qui a produit cette entrée (hostname Windows)
    public string Machine { get; set; } = Environment.MachineName;

    // Nom du job de sauvegarde qui a déclenché le transfert
    public string Name { get; set; } = "";

    // Chemin absolu du fichier source
    public string FileSource { get; set; } = "";

    // Chemin absolu du fichier de destination
    public string FileTarget { get; set; } = "";

    // Taille du fichier transféré en octets
    public long FileSize { get; set; }

    // Temps de transfert du fichier en millisecondes
    public double FileTransferTime { get; set; }

    // Horodatage du transfert au format "yyyy-MM-dd HH:mm:ss"
    [JsonPropertyName("time")]
    public string Timestamp { get; set; } = "";

    // Durée du chiffrement en ms. 0 = pas de chiffrement, >0 = succès, <0 = erreur
    public long EncryptionTime { get; set; }
}
