namespace EasySave.Core;

public interface IBackupStrategy
{
    void Execute(BackupJob job, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseGate = null);
}
