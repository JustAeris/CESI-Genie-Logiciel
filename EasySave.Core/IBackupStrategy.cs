namespace EasySave.Core;

public interface IBackupStrategy
{
<<<<<<< HEAD
    void Execute(BackupJob job, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseGate = null);
=======
    void Execute(BackupJob job, BackupState state, CancellationToken token = default);
>>>>>>> fac1da1 (fix: resolve merge conflict in BackupPipelineTests)
}
