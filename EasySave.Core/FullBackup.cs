namespace EasySave.Core;

public class FullBackup : BackupStrategyBase, IBackupStrategy
{
    /// <summary>Copies all files from source to target recursively.</summary>
<<<<<<< HEAD
    public void Execute(BackupJob job, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseGate = null)
=======
    public void Execute(BackupJob job, BackupState state, CancellationToken token = default)
>>>>>>> fac1da1 (fix: resolve merge conflict in BackupPipelineTests)
    {
        var files = Directory.GetFiles(job.SourceDir, "*", SearchOption.AllDirectories);

        state.TotalFilesToCopy = files.Length;
        state.TotalFilesSize = files.Sum(f => new FileInfo(f).Length);
        state.NbFilesLeftToDo = files.Length;
        state.SizeLeft = state.TotalFilesSize;
        state.State = "ACTIVE";

<<<<<<< HEAD
=======
        // Sort priority files first, then bulk-register them before any copy starts.
        // This ensures the gate is closed before non-priority files are reached,
        // even when multiple jobs run in parallel.
>>>>>>> fac1da1 (fix: resolve merge conflict in BackupPipelineTests)
        var priorityExts = ConfigManager.Instance.Config.PriorityExtensions;
        if (priorityExts.Count > 0)
            files = [.. files.OrderByDescending(f =>
                priorityExts.Any(e => e.Equals(Path.GetExtension(f), StringComparison.OrdinalIgnoreCase)))];

        RegisterPriorityFiles(files);

        foreach (var src in files)
        {
            token.ThrowIfCancellationRequested();
<<<<<<< HEAD
            WaitIfBlockedByPriority(src);
            var dst = BuildDestPath(src, job.SourceDir, job.TargetDir);
            CopyFile(src, dst, state, token, pauseGate);
=======
            WaitIfBlockedByPriority(src); // blocks non-priority if priority files are pending
            var dst = BuildDestPath(src, job.SourceDir, job.TargetDir);
            CopyFile(src, dst, state, token);
>>>>>>> fac1da1 (fix: resolve merge conflict in BackupPipelineTests)
        }

        state.State = "END";
        state.SourceFilePath = "";
        state.TargetFilePath = "";
        StateManager.Instance.Update(state);
    }
}
