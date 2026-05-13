namespace EasySave.Core;

public class FullBackup : BackupStrategyBase, IBackupStrategy
{
    /// <summary>Copies all files from source to target recursively.</summary>
    public void Execute(BackupJob job, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseGate = null)
    {
        var files = Directory.GetFiles(job.SourceDir, "*", SearchOption.AllDirectories);

        state.TotalFilesToCopy = files.Length;
        state.TotalFilesSize = files.Sum(f => new FileInfo(f).Length);
        state.NbFilesLeftToDo = files.Length;
        state.SizeLeft = state.TotalFilesSize;
        state.State = "ACTIVE";

        var priorityExts = ConfigManager.Instance.Config.PriorityExtensions;
        if (priorityExts.Count > 0)
            files = [.. files.OrderByDescending(f =>
                priorityExts.Any(e => e.Equals(Path.GetExtension(f), StringComparison.OrdinalIgnoreCase)))];

        RegisterPriorityFiles(files);

        foreach (var src in files)
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
