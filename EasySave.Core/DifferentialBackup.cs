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

        foreach (var src in filesToCopy)
        {
            var dst = BuildDestPath(src, job.SourceDir, job.TargetDir);
            CopyFile(src, dst, state, token);
        }

        state.State = "END";
        state.SourceFilePath = "";
        state.TargetFilePath = "";
        StateManager.Instance.Update(state);
    }
}
