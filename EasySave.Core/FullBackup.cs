namespace EasySave.Core;

public class FullBackup : BackupStrategyBase, IBackupStrategy
{
    /// <summary>Copies all files from source to target recursively.</summary>
    public void Execute(BackupJob job, BackupState state)
    {
        var files = Directory.GetFiles(job.SourceDir, "*", SearchOption.AllDirectories);

        state.TotalFilesToCopy = files.Length;
        state.TotalFilesSize = files.Sum(f => new FileInfo(f).Length);
        state.NbFilesLeftToDo = files.Length;
        state.SizeLeft = state.TotalFilesSize;
        state.State = "ACTIVE";

        foreach (var src in files)
        {
            var dst = BuildDestPath(src, job.SourceDir, job.TargetDir);
            CopyFile(src, dst, state);
        }

        state.State = "END";
        state.SourceFilePath = "";
        state.TargetFilePath = "";
        StateManager.Instance.Update(state);
    }
}
