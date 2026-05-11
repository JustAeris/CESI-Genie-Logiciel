namespace EasySave.Core;

public interface IBackupStrategy
{
<<<<<<< HEAD
<<<<<<< HEAD
    void Execute(BackupJob job, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseGate = null);
=======
    void Execute(BackupJob job, BackupState state, CancellationToken token = default);
>>>>>>> fac1da1 (fix: resolve merge conflict in BackupPipelineTests)
}
=======
    void Execute(BackupJob job, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseEvent = null);
}
>>>>>>> d36cf20 (Add Pause/Resume/Stop controls (T5))
