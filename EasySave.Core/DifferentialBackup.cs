namespace EasySave.Core;

public class DifferentialBackup : BackupStrategyBase, IBackupStrategy
{
    /// <summary>Copies only files that are new or modified since the last backup.</summary>
<<<<<<< HEAD
    public void Execute(BackupJob job, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseGate = null)
=======
    public void Execute(BackupJob job, BackupState state, CancellationToken token = default)
>>>>>>> fac1da1 (fix: resolve merge conflict in BackupPipelineTests)
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

<<<<<<< HEAD
=======
        // Sort priority files first, then bulk-register them before any copy starts.
>>>>>>> fac1da1 (fix: resolve merge conflict in BackupPipelineTests)
        var priorityExts = ConfigManager.Instance.Config.PriorityExtensions;
        if (priorityExts.Count > 0)
            filesToCopy = [.. filesToCopy.OrderByDescending(f =>
                priorityExts.Any(e => e.Equals(Path.GetExtension(f), StringComparison.OrdinalIgnoreCase)))];

        RegisterPriorityFiles(filesToCopy);

        foreach (var src in filesToCopy)
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
