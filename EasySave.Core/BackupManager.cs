namespace EasySave.Core;

public class BackupManager
{
    private static readonly Lazy<BackupManager> _instance = new(() => new BackupManager());
    public static BackupManager Instance => _instance.Value;

    private readonly List<BackupJob> _jobs = [];

    public void AddJob(BackupJob job) => _jobs.Add(job);

    public void RemoveJob(int index)
    {
        if (index < 1 || index > _jobs.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        _jobs.RemoveAt(index - 1);
    }

    public void RunJob(int index)
    {
        if (index < 1 || index > _jobs.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        RunJob(_jobs[index - 1]);
    }

    public void RunAll()
    {
        foreach (var job in _jobs)
            RunJob(job);
    }

    private void RunJob(BackupJob job)
    {
        var strategy = GetStrategy(job.Type);
        var state = new BackupState { Name = job.Name };
        strategy.Execute(job, state);
    }

    private IBackupStrategy GetStrategy(BackupType type) => type switch
    {
        BackupType.Full => new FullBackup(),
        BackupType.Differential => new DifferentialBackup(),
        _ => throw new ArgumentOutOfRangeException()
    };
}
