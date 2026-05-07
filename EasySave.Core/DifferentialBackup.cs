namespace EasySave.Core;

public class DifferentialBackup : BackupStrategyBase, IBackupStrategy
{
    /// <summary>Copies only files that are new or modified since the last backup.</summary>
    public void Execute(BackupJob job, BackupState state, CancellationToken token = default)
    {
        var allFiles = Directory.GetFiles(job.SourceDir, "*", SearchOption.AllDirectories);

        var filesToCopy = allFiles.Where(src =>
        {
            var dst = BuildDestPath(src, job.SourceDir, job.TargetDir);
            // Only copy if destination doesn't exist or source is newer
            bool needsCopy = !File.Exists(dst) || File.GetLastWriteTime(src) > File.GetLastWriteTime(dst);
            return needsCopy;
        }).ToArray();

        state.TotalFilesToCopy = filesToCopy.Length;
        state.TotalFilesSize = filesToCopy.Sum(f => new FileInfo(f).Length);
        state.NbFilesLeftToDo = filesToCopy.Length;
        state.SizeLeft = state.TotalFilesSize;
        state.State = "ACTIVE";

        // Priority files first so the gate closes before any non-priority file is reached
        var priorityExts = ConfigManager.Instance.Config.PriorityExtensions;
        if (priorityExts.Count > 0)
            filesToCopy = [.. filesToCopy.OrderByDescending(f =>
                priorityExts.Any(e => e.Equals(Path.GetExtension(f), StringComparison.OrdinalIgnoreCase)))];

        foreach (var src in filesToCopy)
        {
            token.ThrowIfCancellationRequested();
            WaitIfBlockedByPriority(src); // blocks non-priority if priority files are pending
            var dst = BuildDestPath(src, job.SourceDir, job.TargetDir);
            CopyFile(src, dst, state, token);
        }

        state.State = "END";
        state.SourceFilePath = "";
        state.TargetFilePath = "";
        StateManager.Instance.Update(state);
    }
}
