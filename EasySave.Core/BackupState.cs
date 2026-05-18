namespace EasySave.Core;

// Instantané de la progression d'un job, persisté dans state.json / state.xml par StateManager.
// Mis à jour après chaque fichier copié ; lu par l'interface graphique toutes les 500 ms pour rafraîchir la barre de progression.
public class BackupState
{
    // Verrou pour rendre les mises à jour des compteurs atomiques entre les threads
    private readonly object _lock = new();

    // Nom unique qui relie cet état à son BackupJob
    public string Name { get; set; } = "";

    // Cycle de vie global : "IDLE" | "ACTIVE" | "END"
    public string State { get; set; } = "IDLE";

    // Nombre total de fichiers à copier au lancement du job
    public int TotalFilesToCopy { get; set; }

    // Taille totale en octets de tous les fichiers à copier
    public long TotalFilesSize { get; set; }

    // Nombre de fichiers restant à copier
    public int NbFilesLeftToDo { get; set; }

    // Nombre d'octets restant à copier
    public long SizeLeft { get; set; }

    // Pourcentage de fichiers copiés, de 0,0 à 100,0
    public double Progression { get; set; }

    // Chemin source du fichier actuellement en cours de copie
    public string SourceFilePath { get; set; } = "";

    // Chemin de destination du fichier actuellement en cours de copie
    public string TargetFilePath { get; set; } = "";

    // Horodatage de la dernière mise à jour de l'état
    public DateTime Timestamp { get; set; }

    // État de contrôle fin : Running | Paused | Stopped (T5)
    public PlaybackState PlaybackState { get; set; } = PlaybackState.Stopped;

    // Met à jour les compteurs de progression de façon atomique après la copie d'un fichier.
    // Recalcule Progression en pourcentage de fichiers traités par rapport au total.
    public void DecrementProgress(long fileSize, int totalFiles)
    {
        lock (_lock)
        {
            NbFilesLeftToDo--;
            SizeLeft -= fileSize;
            // Protection contre la division par zéro si le dossier source était vide
            Progression = totalFiles == 0
                ? 100.0
                : (totalFiles - NbFilesLeftToDo) / (double)totalFiles * 100.0;
        }
    }
}
