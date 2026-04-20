namespace EasySave.Core;

public class BackupManager
{
    public void RunJob(BackupJob job)
    {
        throw new NotImplementedException();
    }

    private IBackupStrategy GetStrategy(BackupType type) => type switch
    {
        BackupType.Full         => new FullBackup(),
        BackupType.Differential => new DifferentialBackup(),
        _                       => throw new ArgumentOutOfRangeException()
    };
}
