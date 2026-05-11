namespace EasySave.Core;

public class DifferentialBackup : BackupStrategyBase, IBackupStrategy
{
    /// <summary>Copies only files that are new or modified since the last backup.</summary>
    public void Execute(BackupJob job, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseGate = null)
    {
        var allFiles = Directory.GetFiles(job.SourceDir, "*", SearchOption.AllDirectories);

        var filesToCopy = allFiles.Where(src =>
        {
            var dst = BuildDestPath(src, job.SourceDir, job.TargetDir);
            return !File.Exists(dst) || File.GetLastWriteTime(src) > File.GetLastWriteTime(dst);
        }).ToArray();

        state.TotalFilesToCopy = filesToCopy.Length;
        state.TotalFilesSize = filesToCopy.Sum(f => new FileInfo(f).Length);
        state.NbFilesLeftToDo = filesToCopy.Length;
        state.SizeLeft = state.TotalFilesSize;
        state.State = "ACTIVE";

        var priorityExts = ConfigManager.Instance.Config.PriorityExtensions;
        if (priorityExts.Count > 0)
            filesToCopy = [.. filesToCopy.OrderByDescending(f =>
                priorityExts.Any(e => e.Equals(Path.GetExtension(f), StringComparison.OrdinalIgnoreCase)))];

        RegisterPriorityFiles(filesToCopy);

        foreach (var src in filesToCopy)
        {
            token.ThrowIfCancellationRequested();
            WaitIfBlockedByPriority(src);
            var dst = BuildDestPath(src, job.SourceDir, job.TargetDir);
            CopyFile(src, dst, state, token, pauseGate);
        }

        state.State = "END";
        state.SourceFilePath = "";
        state.TargetFilePath = "";
        StateManager.Instance.Update(state);
    }
}
