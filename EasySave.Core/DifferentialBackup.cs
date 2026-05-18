namespace EasySave.Core;

// Stratégie de sauvegarde différentielle — copie uniquement les fichiers nouveaux ou modifiés depuis la dernière sauvegarde.
public class DifferentialBackup : BackupStrategyBase, IBackupStrategy
{
    // Copie les fichiers absents de la destination ou dont la source est plus récente que la copie existante.
    public void Execute(BackupJob job, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseGate = null)
    {
        // Énumère tous les fichiers du dossier source de façon récursive
        var allFiles = Directory.GetFiles(job.SourceDir, "*", SearchOption.AllDirectories);

        // Filtre : ne garde que les fichiers absents de la destination ou plus récents que leur copie
        var filesToCopy = allFiles.Where(src =>
        {
            var dst = BuildDestPath(src, job.SourceDir, job.TargetDir);
            return !File.Exists(dst) || File.GetLastWriteTime(src) > File.GetLastWriteTime(dst);
        }).ToArray();

        // Initialise les compteurs de progression avec le nombre réel de fichiers à copier
        state.TotalFilesToCopy = filesToCopy.Length;
        state.TotalFilesSize = filesToCopy.Sum(f => new FileInfo(f).Length);
        state.NbFilesLeftToDo = filesToCopy.Length;
        state.SizeLeft = state.TotalFilesSize;
        state.State = "ACTIVE";

        // Trie les fichiers prioritaires en tête de liste pour qu'ils soient copiés en premier (T8)
        var priorityExts = ConfigManager.Instance.Config.PriorityExtensions;
        if (priorityExts.Count > 0)
            filesToCopy = [.. filesToCopy.OrderByDescending(f =>
                priorityExts.Any(e => e.Equals(Path.GetExtension(f), StringComparison.OrdinalIgnoreCase)))];

        // Enregistre les fichiers prioritaires pour bloquer les fichiers non-prioritaires des autres jobs
        RegisterPriorityFiles(filesToCopy);

        foreach (var src in filesToCopy)
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
