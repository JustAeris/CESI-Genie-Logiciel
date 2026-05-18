namespace EasySave.Core;

// Stratégie de sauvegarde complète — copie tous les fichiers du dossier source vers la destination.
public class FullBackup : BackupStrategyBase, IBackupStrategy
{
    // Copie récursivement tous les fichiers de la source, sans condition sur la date de modification.
    public void Execute(BackupJob job, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseGate = null)
    {
        // Récupère tous les fichiers du dossier source de façon récursive
        var files = Directory.GetFiles(job.SourceDir, "*", SearchOption.AllDirectories);

        // Initialise les compteurs de progression avant de démarrer la copie
        state.TotalFilesToCopy = files.Length;
        state.TotalFilesSize = files.Sum(f => new FileInfo(f).Length);
        state.NbFilesLeftToDo = files.Length;
        state.SizeLeft = state.TotalFilesSize;
        state.State = "ACTIVE";

        // Trie les fichiers prioritaires en tête de liste pour qu'ils soient copiés en premier (T8)
        var priorityExts = ConfigManager.Instance.Config.PriorityExtensions;
        if (priorityExts.Count > 0)
            files = [.. files.OrderByDescending(f =>
                priorityExts.Any(e => e.Equals(Path.GetExtension(f), StringComparison.OrdinalIgnoreCase)))];

        // Enregistre les fichiers prioritaires pour bloquer les fichiers non-prioritaires des autres jobs
        RegisterPriorityFiles(files);

        foreach (var src in files)
        {
            token.ThrowIfCancellationRequested(); // respecte les demandes d'arrêt entre chaque fichier
            WaitIfBlockedByPriority(src);         // attend si ce fichier est non-prioritaire et qu'il reste des prioritaires
            var dst = BuildDestPath(src, job.SourceDir, job.TargetDir);
            CopyFile(src, dst, state, token, pauseGate);
        }

        // Marque le job comme terminé et efface les chemins du fichier en cours
        state.State = "END";
        state.SourceFilePath = "";
        state.TargetFilePath = "";
        StateManager.Instance.Update(state);
    }
}
