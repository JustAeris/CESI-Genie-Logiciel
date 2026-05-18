namespace EasySave.Core;

// Représente un job de sauvegarde configuré, stocké dans AppConfig.Jobs et persisté dans config.json.
public class BackupJob
{
    // Nom d'affichage du job, utilisé aussi comme identifiant unique dans BackupManager
    public string Name { get; set; } = "";

    // Chemin absolu du dossier à sauvegarder
    public string SourceDir { get; set; } = "";

    // Chemin absolu du dossier de destination où la sauvegarde est écrite
    public string TargetDir { get; set; } = "";

    // Full = copie tous les fichiers ; Differential = copie uniquement les fichiers nouveaux ou modifiés
    public BackupType Type { get; set; }
}
